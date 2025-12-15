document.addEventListener("DOMContentLoaded", function () {

    console.log("portfolioData:", portfolioData);

    // helpers
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const antiforgery = tokenInput ? tokenInput.value : null;

    const tbody = document.getElementById("portfolioTbody");
    const totalValueEl = document.getElementById("totalValue");

    function formatCurrency(v) {
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(v);
    }

    function renderTable(portfolio) {
        if (!portfolio) return;
        tbody.innerHTML = '';

        for (const asset of portfolio.assets) {
            const tr = document.createElement('tr');
            tr.setAttribute('data-symbol', asset.symbol);

            tr.innerHTML = `
                <td>${asset.symbol}</td>
                <td>${asset.name}</td>
                <td class="text-end">${asset.quantity}</td>
                <td class="text-end">${formatCurrency(asset.price)}</td>
                <td class="text-end">${formatCurrency(asset.value)}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-danger delete-btn" data-symbol="${asset.symbol}">✕</button>
                </td>
            `;
            tbody.appendChild(tr);
        }

        totalValueEl.textContent = formatCurrency(portfolio.totalValue);
    }

    // initial render (server-side provided)
    renderTable(portfolioData);

    // delegate delete click
    tbody.addEventListener('click', async (e) => {
        const btn = e.target.closest('.delete-btn');
        if (!btn) return;

        const symbol = btn.dataset.symbol;
        if (!confirm(`Delete ${symbol} from portfolio?`)) return;

        const response = await fetch(`/Portfolio?handler=Delete&symbol=${encodeURIComponent(symbol)}`, {
            method: 'POST',
            headers: antiforgery ? { 'RequestVerificationToken': antiforgery } : {}
        });

        if (!response.ok) {
            alert('Error deleting asset.');
            return;
        }

        const updated = await response.json();
        renderTable(updated);
    });

    // add asset
    document.getElementById("addAssetBtn").addEventListener('click', async (e) => {
        e.preventDefault();

        const symbol = document.getElementById("symbolSelect").value;
        const name = document.getElementById("symbolSelect").selectedOptions[0].text.split(" - ").slice(1).join(" - ").trim() || symbol;
        const quantity = parseInt(document.getElementById("quantityInput").value, 10);

        if (!quantity || quantity <= 0) {
            alert("Quantity must be > 0");
            return;
        }

        const body = { symbol, name, quantity };

        const response = await fetch("/Portfolio?handler=Add", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(antiforgery ? { 'RequestVerificationToken': antiforgery } : {})
            },
            body: JSON.stringify(body)
        });

        if (!response.ok) {
            const txt = await response.text();
            alert("Error adding asset: " + txt);
            return;
        }

        const updated = await response.json();
        renderTable(updated);
    });

    // simulate market update
    const simulateBtn = document.getElementById("simulateBtn");

    if (simulateBtn) {
        simulateBtn.addEventListener("click", async () => {

            console.log("👉 CLICK: Simulate Market Update triggered");

            const btn = simulateBtn;
            btn.disabled = true;
            btn.textContent = "Simulating...";

            console.log("👉 Sending POST to /Portfolio?handler=Simulate");

            const response = await fetch("/Portfolio?handler=Simulate", {
                method: "POST",
                headers: antiforgery ? { "RequestVerificationToken": antiforgery } : {}
            }).catch(err => {
                console.error("❌ Network error during fetch:", err);
                alert("Network error — check console.");
                btn.disabled = false;
                btn.textContent = "Simulate Market Update";
                return null;
            });

            console.log("👉 Raw response:", response);

            if (!response || !response.ok) {
                console.error("❌ Response not OK:", response?.status, response?.statusText);
                alert("Simulation failed: HTTP error. Check console.");
                btn.disabled = false;
                btn.textContent = "Simulate Market Update";
                return;
            }

            let result;
            try {
                result = await response.json();
                console.log("👉 Parsed JSON:", result);

                // ✅ Render updated portfolio
                if (result && result.portfolio) {
                    renderTable(result.portfolio);
                }

                if (result && result.timestamp) {
                    updateLastSimulatedAt(result.timestamp);
                }

            } catch (e) {
                console.error("❌ JSON parse error:", e);
                alert("Simulation returned invalid JSON — check console.");
            } finally {
                btn.disabled = false;
                btn.textContent = "Simulate Market Update";
            }
        });
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
});