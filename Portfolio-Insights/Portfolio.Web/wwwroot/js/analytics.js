
document.addEventListener("DOMContentLoaded", () => {

    async function fetchAnalyticsWithRetry(maxRetries = 5, delayMs = 300) {
        for (let attempt = 1; attempt <= maxRetries; attempt++) {
            const res = await fetch("/Analytics?handler=Get", {
                headers: token ? { "RequestVerificationToken": token } : {}
            });

            if (!res.ok) throw new Error(`HTTP ${res.status}`);

            const data = await res.json();

            const analytics = normalizeAnalytics(data.analytics ?? data.Analytics ?? data);
            const history = normalizeHistory(data.history ?? data.History ?? []);

            // ✅ consider analytics "ready" only if it has data
            if (
                analytics.totalValue > 0 &&
                analytics.assetContributions.length > 0 &&
                history.length > 0
            ) {
                return { analytics, history };
            }

            console.log(`⏳ Analytics not ready (attempt ${attempt}), retrying...`);
            await new Promise(r => setTimeout(r, delayMs));
        }

        console.warn("⚠️ Analytics still not ready after retries");
        return null;
    }

    function updateLastSimulatedAt(timestamp) {
        console.log("updateLastSimulatedAt called with:", timestamp);

        if (!timestamp) return;

        const el = document.getElementById("lastSimulatedAt");
        if (!el) {
            console.warn("lastSimulatedAt element NOT FOUND");
            return;
        }

        const date = new Date(timestamp);
        el.textContent = "Last update: " + date.toLocaleString();
    }
    // --- Normalize incoming data (PascalCase <-> camelCase handling) ---
    function normalizeAnalytics(raw) {
        if (!raw) return {
            totalValue: 0,
            dailyChangePercent: 0,
            totalReturnPercent: 0,
            assetContributions: []
        };

        const get = (obj, pascal, camel) => {
            if (obj === null || obj === undefined) return undefined;
            if (obj[camel] !== undefined) return obj[camel];
            if (obj[pascal] !== undefined) return obj[pascal];
            return undefined;
        };

        const totalValue = get(raw, "TotalValue", "totalValue") ?? 0;
        const dailyChangePercent = get(raw, "DailyChangePercent", "dailyChangePercent") ?? 0;
        const totalReturnPercent = get(raw, "TotalReturnPercent", "totalReturnPercent") ?? 0;
        const acRaw = get(raw, "AssetContributions", "assetContributions") ?? [];

        const assetContributions = (acRaw || []).map(a => {
            const symbol = a.symbol ?? a.Symbol ?? "";
            const value = Number(a.value ?? a.Value ?? a.currentValue ?? a.CurrentValue ?? 0);
            const weightPercent = Number(a.weightPercent ?? a.WeightPercent ?? a.weight ?? a.Weight ?? 0);
            return { symbol, value, weightPercent };
        });

        const lastUpdated = get(raw, "LastUpdated", "lastUpdated") ?? null;

        return { totalValue, dailyChangePercent, totalReturnPercent, assetContributions, lastUpdated };
    }

    function normalizeHistory(rawHistory) {
        if (!rawHistory || !Array.isArray(rawHistory)) return [];
        return rawHistory.map(h => {
            const timestamp = h.timestamp ?? h.Timestamp ?? h.time ?? h.Time ?? null;
            const totalValue = Number(h.totalValue ?? h.TotalValue ?? h.value ?? h.Value ?? 0);
            return { timestamp, totalValue };
        });
    }

    // --- Get Razor-injected data ---
    const normAnalytics = normalizeAnalytics(typeof analyticsData !== "undefined" ? analyticsData : null);
    const normHistory = normalizeHistory(typeof historyData !== "undefined" ? historyData : []);

    const token = document.querySelector("input[name='__RequestVerificationToken']")?.value;

    const updatedAtEl = document.getElementById("updatedAt");
    const pieCtx = document.getElementById("pieChart");
    const lineCtx = document.getElementById("lineChart");
    const contribTable = document.getElementById("contribTable");

    if (!pieCtx || !lineCtx || !contribTable) {
        console.error("analytics.js: required DOM elements missing. Aborting.");
        return;
    }

    let pieChart = null;
    let lineChart = null;

    // -------------------------------
    // Helpers
    // -------------------------------
    function formatCurrency(v) {
        if (typeof v !== "number" || isNaN(v)) return "$0.00";
        return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
    }

    // -------------------------------
    // Metrics
    // -------------------------------
    function updateMetrics(a) {
        document.getElementById("m_totalValue").textContent = formatCurrency(a.totalValue ?? 0);

        const dc = document.getElementById("m_dailyChange");
        const dcVal = (a.dailyChangePercent ?? 0);
        dc.textContent = dcVal.toFixed(2) + "%";
        dc.className = "fw-bold " + (dcVal >= 0 ? "text-success" : "text-danger");

        document.getElementById("m_totalReturn").textContent = (a.totalReturnPercent ?? 0).toFixed(2) + "%";
    }

    // -------------------------------
    // Pie Chart
    // -------------------------------
    function updatePieChart(contrib) {
        if (pieChart) { pieChart.destroy(); pieChart = null; }
        const arr = contrib || [];
        if (!arr.length) return;
        const labels = arr.map(x => x.symbol);
        const values = arr.map(x => Number(x.weightPercent || 0));

        pieChart = new Chart(pieCtx, {
            type: "pie",
            data: { labels, datasets: [{ data: values }] },
            options: { responsive: true, maintainAspectRatio: false }
        });
    }

    // -------------------------------
    // Line Chart
    // -------------------------------
    function updateLineChart(history) {
        if (lineChart) { lineChart.destroy(); lineChart = null; }
        const h = history || [];
        if (!h.length) return;

        const labels = h.map(s => s.timestamp ? new Date(s.timestamp).toLocaleTimeString() : new Date().toLocaleTimeString());
        const values = h.map(s => Number(s.totalValue || 0));

        lineChart = new Chart(lineCtx, {
            type: "line",
            data: { labels, datasets: [{ label: "Total Value", data: values, tension: 0.3 }] },
            options: { responsive: true, maintainAspectRatio: false }
        });
    }

    // -------------------------------
    // Contribution Table
    // -------------------------------
    function updateContribTable(contrib) {
        contribTable.innerHTML = "";
        const arr = contrib || [];
        if (!arr.length) {
            const tr = document.createElement("tr");
            tr.innerHTML = `<td colspan="3" class="text-center text-muted">No assets</td>`;
            contribTable.appendChild(tr);
            return;
        }
        arr.forEach(a => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${a.symbol ?? ""}</td>
                <td class="text-end">${formatCurrency(a.value ?? 0)}</td>
                <td class="text-end">${(a.weightPercent ?? 0).toFixed(2)}%</td>
            `;
            contribTable.appendChild(tr);
        });
    }

    // -------------------------------
    // Master render
    // -------------------------------
    function renderAll(analytics, history) {
        if (!analytics || !history || history.length === 0) {
            console.warn("Analytics not ready — skipping render");
            return;
        }

        updateMetrics(analytics);
        updatePieChart(analytics.assetContributions || []);
        updateLineChart(history || []);
        updateContribTable(analytics.assetContributions || []);
        updatedAtEl.textContent =
            analytics.lastUpdated
                ? new Date(analytics.lastUpdated).toLocaleString()
                : new Date().toLocaleString();
    }

    // Initial render
    renderAll(normAnalytics, normHistory);

    // -------------------------------
    // Simulate button
    // -------------------------------
    const simulateBtn = document.getElementById("simulateBtn");
    if (simulateBtn) {
        simulateBtn.addEventListener("click", async () => {
            simulateBtn.disabled = true;
            const originalText = simulateBtn.textContent;
            simulateBtn.textContent = "Simulating...";

            //try {
            //    const res = await fetch("/Analytics?handler=Simulate", {
            //        method: "POST",
            //        headers: token ? { "RequestVerificationToken": token } : {}
            //    });

            //    if (!res.ok) throw new Error(`HTTP ${res.status}`);

            //    // 🔁 Wait until analytics is actually ready
            //    const result = await fetchAnalyticsWithRetry();

            //    if (!result) {
            //        alert("Analytics update is taking longer than expected. Please refresh.");
            //        return;
            //    }

            //    renderAll(result.analytics, result.history);

            //} catch (err) {
            //    console.error("Simulation failed:", err);
            //    alert("Simulation failed. See console for details.");
            //}


            try {
                const res = await fetch("/Analytics?handler=Simulate", {
                    method: "POST",
                    headers: token ? { "RequestVerificationToken": token } : {}
                });

                if (!res.ok) throw new Error(`HTTP ${res.status}`);

                const payload = await res.json();
                const newNormAnalytics = normalizeAnalytics(payload.analytics ?? payload.Analytics ?? null);
                const newNormHistory = normalizeHistory(payload.history ?? payload.History ?? []);
                renderAll(newNormAnalytics, newNormHistory);

            } catch (err) {
                console.error("Simulation failed:", err);
                alert("Simulation failed. See console for details.");
            } finally {
                simulateBtn.disabled = false;
                simulateBtn.textContent = originalText;
            }
        });
    }
});