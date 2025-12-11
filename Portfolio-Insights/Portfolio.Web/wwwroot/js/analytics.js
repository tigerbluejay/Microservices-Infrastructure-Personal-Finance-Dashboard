//document.addEventListener("DOMContentLoaded", () => {

//    // --- Normalize incoming data (PascalCase <-> camelCase handling) ---
//    function normalizeAnalytics(raw) {
//        if (!raw) return {
//            totalValue: 0,
//            dailyChangePercent: 0,
//            totalReturnPercent: 0,
//            assetContributions: []
//        };

//        // Helpers to read either casing
//        const get = (obj, pascal, camel) => {
//            if (obj === null || obj === undefined) return undefined;
//            if (obj[camel] !== undefined) return obj[camel];
//            if (obj[pascal] !== undefined) return obj[pascal];
//            return undefined;
//        };

//        const totalValue = get(raw, "TotalValue", "totalValue") ?? 0;
//        const dailyChangePercent = get(raw, "DailyChangePercent", "dailyChangePercent") ?? 0;
//        const totalReturnPercent = get(raw, "TotalReturnPercent", "totalReturnPercent") ?? 0;
//        const acRaw = get(raw, "AssetContributions", "assetContributions") ?? [];

//        const assetContributions = (acRaw || []).map(a => {
//            // support several field name variants
//            const symbol = a.symbol ?? a.Symbol ?? "";
//            const value = Number(a.value ?? a.Value ?? a.currentValue ?? a.CurrentValue ?? 0);
//            const weightPercent = Number(a.weightPercent ?? a.WeightPercent ?? a.weight ?? a.Weight ?? 0);
//            return { symbol, value, weightPercent };
//        });

//        // Also include optional lastUpdated if present
//        const lastUpdated = get(raw, "LastUpdated", "lastUpdated") ?? null;

//        return { totalValue, dailyChangePercent, totalReturnPercent, assetContributions, lastUpdated };
//    }

//    function normalizeHistory(rawHistory) {
//        if (!rawHistory || !Array.isArray(rawHistory)) return [];
//        return rawHistory.map(h => {
//            const timestamp = h.timestamp ?? h.Timestamp ?? h.time ?? h.Time ?? null;
//            const totalValue = Number(h.totalValue ?? h.TotalValue ?? h.value ?? h.Value ?? 0);
//            return { timestamp, totalValue };
//        });
//    }

//    // --- Use normalized data from the Razor-injected variables ---
//    // analyticsData and historyData come from the Razor page
//    console.log("raw analyticsData (from Razor):", typeof analyticsData !== "undefined" ? analyticsData : "(undefined)");
//    console.log("raw historyData (from Razor):", typeof historyData !== "undefined" ? historyData : "(undefined)");

//    const normAnalytics = normalizeAnalytics(typeof analyticsData !== "undefined" ? analyticsData : null);
//    const normHistory = normalizeHistory(typeof historyData !== "undefined" ? historyData : []);

//    console.log("normalized analytics:", normAnalytics);
//    console.log("normalized history:", normHistory);

//    // Keep token as before
//    const token = document.querySelector("input[name='__RequestVerificationToken']")?.value;

//    const updatedAtEl = document.getElementById("updatedAt");
//    const pieCtx = document.getElementById("pieChart");
//    const lineCtx = document.getElementById("lineChart");
//    const contribTable = document.getElementById("contribTable");

//    if (!pieCtx || !lineCtx || !contribTable) {
//        console.error("analytics.js: required DOM elements missing (pieChart, lineChart, contribTable). Aborting.");
//        return;
//    }

//    let pieChart = null;
//    let lineChart = null;

//    // -------------------------------
//    // Helpers
//    // -------------------------------
//    function formatCurrency(v) {
//        if (typeof v !== "number" || isNaN(v)) return "$0.00";
//        return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
//    }

//    // -------------------------------
//    // Key Metrics
//    // -------------------------------
//    function updateMetrics(a) {
//        document.getElementById("m_totalValue").textContent = formatCurrency(a.totalValue ?? 0);

//        const dc = document.getElementById("m_dailyChange");
//        const dcVal = (a.dailyChangePercent ?? 0);
//        dc.textContent = dcVal.toFixed(2) + "%";
//        dc.className = "fw-bold " + (dcVal >= 0 ? "text-success" : "text-danger");

//        document.getElementById("m_totalReturn").textContent = (a.totalReturnPercent ?? 0).toFixed(2) + "%";
//    }

//    // -------------------------------
//    // Pie Chart
//    // -------------------------------
//    function updatePieChart(contrib) {
//        if (pieChart) { pieChart.destroy(); pieChart = null; }

//        const arr = contrib || [];
//        if (!arr.length) {
//            // nothing to render
//            return;
//        }

//        const labels = arr.map(x => x.symbol);
//        const values = arr.map(x => Number(x.weightPercent || 0));

//        pieChart = new Chart(pieCtx, {
//            type: "pie",
//            data: {
//                labels,
//                datasets: [{
//                    data: values
//                }]
//            },
//            options: { responsive: true, maintainAspectRatio: false }
//        });
//    }

//    // -------------------------------
//    // Line Chart
//    // -------------------------------
//    function updateLineChart(history) {
//        if (lineChart) { lineChart.destroy(); lineChart = null; }

//        const h = history || [];
//        if (!h.length) return;

//        const labels = h.map(s => {
//            // convert possible timestamp types safely
//            const t = s.timestamp;
//            const d = t ? new Date(t) : new Date();
//            return d.toLocaleTimeString();
//        });
//        const values = h.map(s => Number(s.totalValue || 0));

//        lineChart = new Chart(lineCtx, {
//            type: "line",
//            data: {
//                labels,
//                datasets: [{
//                    label: "Total Value",
//                    data: values,
//                    tension: 0.3
//                }]
//            },
//            options: { responsive: true, maintainAspectRatio: false }
//        });
//    }

//    // -------------------------------
//    // Contribution Table
//    // -------------------------------
//    function updateContribTable(contrib) {
//        contribTable.innerHTML = "";

//        const arr = contrib || [];
//        if (!arr.length) {
//            const tr = document.createElement("tr");
//            tr.innerHTML = `<td colspan="3" class="text-center text-muted">No assets</td>`;
//            contribTable.appendChild(tr);
//            return;
//        }

//        arr.forEach(a => {
//            const tr = document.createElement("tr");
//            const valueNum = Number(a.value ?? 0);
//            const weight = Number(a.weightPercent ?? 0);
//            tr.innerHTML = `
//                <td>${a.symbol ?? ""}</td>
//                <td class="text-end">${formatCurrency(valueNum)}</td>
//                <td class="text-end">${weight.toFixed(2)}%</td>
//            `;
//            contribTable.appendChild(tr);
//        });
//    }

//    // -------------------------------
//    // Master render
//    // -------------------------------
//    function renderAll(analytics, history) {
//        updateMetrics(analytics);
//        updatePieChart(analytics.assetContributions || []);
//        updateLineChart(history || []);
//        updateContribTable(analytics.assetContributions || []);
//        // updatedAt: prefer server-provided lastUpdated if exists
//        if (analytics.lastUpdated) {
//            updatedAtEl.textContent = new Date(analytics.lastUpdated).toLocaleString();
//        } else {
//            updatedAtEl.textContent = new Date().toLocaleString();
//        }
//    }

//    // initial render using normalized data
//    renderAll(normAnalytics, normHistory);

//    // -------------------------------
//    // Simulate handler (keeps your original behavior)
//    // -------------------------------
//    const simulateBtn = document.getElementById("simulateBtn");
//    if (simulateBtn) {
//        simulateBtn.addEventListener("click", async () => {
//            simulateBtn.disabled = true;
//            const originalText = simulateBtn.textContent;
//            simulateBtn.textContent = "Simulating...";

//            try {
//                const res = await fetch("/Analytics?handler=Simulate", {
//                    method: "POST",
//                    headers: token ? { "RequestVerificationToken": token } : {}
//                });

//                if (!res.ok) {
//                    const body = await res.text().catch(() => "");
//                    console.error("Simulate failed", res.status, body);
//                    alert("Simulation failed (status " + res.status + ").");
//                    return;
//                }

//                const payload = await res.json();
//                const newNormAnalytics = normalizeAnalytics(payload.analytics ?? payload.Analytics ?? null);
//                const newNormHistory = normalizeHistory(payload.history ?? payload.History ?? []);
//                renderAll(newNormAnalytics, newNormHistory);
//            } catch (err) {
//                console.error("Simulate error", err);
//                alert("Simulation failed (network error).");
//            } finally {
//                simulateBtn.disabled = false;
//                simulateBtn.textContent = originalText;
//            }
//        });
//    }
//});

document.addEventListener("DOMContentLoaded", () => {

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
        updateMetrics(analytics);
        updatePieChart(analytics.assetContributions || []);
        updateLineChart(history || []);
        updateContribTable(analytics.assetContributions || []);
        updatedAtEl.textContent = analytics.lastUpdated ? new Date(analytics.lastUpdated).toLocaleString() : new Date().toLocaleString();
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