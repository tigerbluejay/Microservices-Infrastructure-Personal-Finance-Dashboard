document.addEventListener("DOMContentLoaded", function () {

    console.log("portfolioData:", portfolioData);

    // helpers
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const antiforgery = tokenInput ? tokenInput.value : null;

    const tbody = document.getElementById("portfolioTbody");
    const totalValueEl = document.getElementById("totalValue");
    const lastRevalEl = document.getElementById("lastReval");

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

    // revalue
    document.getElementById("revalueBtn").addEventListener('click', async () => {
        const btn = document.getElementById("revalueBtn");
        btn.disabled = true;
        btn.textContent = "Revaluing...";

        const response = await fetch("/Portfolio?handler=Revalue", {
            method: 'POST',
            headers: antiforgery ? { 'RequestVerificationToken': antiforgery } : {}
        });

        if (!response.ok) {
            alert('Error revaluing portfolio.');
            btn.disabled = false;
            btn.textContent = "Revalue Portfolio";
            return;
        }

        const updated = await response.json();
        renderTable(updated);

        // update last revaluation time
        const now = new Date();
        lastRevalEl.textContent = now.toLocaleTimeString();

        btn.disabled = false;
        btn.textContent = "Revalue Portfolio";
    });

    // simulate market update
    document.getElementById("simulateBtn").addEventListener("click", async () => {
        const btn = document.getElementById("simulateBtn");
        btn.disabled = true;
        btn.textContent = "Simulating...";

        const response = await fetch("/Index?handler=Simulate", {
            method: "POST",
            headers: antiforgery ? { "RequestVerificationToken": antiforgery } : {}
        });

        if (!response.ok) {
            alert("Error simulating market update.");
            btn.disabled = false;
            btn.textContent = "Simulate Market Update";
            return;
        }

        const result = await response.json();

        if (!result.success) {
            alert("Simulation failed.");
            btn.disabled = false;
            btn.textContent = "Simulate Market Update";
            return;
        }

        // Fetch updated portfolio immediately after simulation
        const updatedResponse = await fetch("/Portfolio?handler=Get", {
            method: "GET"
        });

        if (updatedResponse.ok) {
            const updated = await updatedResponse.json();
            renderTable(updated);
            lastRevalEl.textContent = new Date().toLocaleTimeString();
            alert("Market simulation completed!");
        } else {
            alert("Simulation succeeded but could not reload portfolio.");
        }

        btn.disabled = false;
        btn.textContent = "Simulate Market Update";
    });

});