document.addEventListener("DOMContentLoaded", function () {

    console.log("historyData from Razor:", historyData);

    if (!historyData || historyData.length === 0) return;

    const labels = historyData.map(x => {
        const safeTimestamp = x.Timestamp.replace(/(\.\d{3})\d+$/, '$1') + 'Z';
        const d = new Date(safeTimestamp);
        return d.toLocaleTimeString();
    });

    const values = historyData.map(x => x.TotalValue);

    const ctx = document.getElementById('miniChart');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Total Value',
                data: values,
                tension: 0.3
            }]
        }
    });

    document.getElementById("simulateBtn")
        ?.addEventListener("click", async () => {

            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            const response = await fetch("/Index?handler=Simulate", {
                method: "POST",
                headers: {
                    "RequestVerificationToken": token
                }
            });

            const data = await response.json();

            if (data.success) {
                alert("✅ Market simulation triggered!");
                setTimeout(() => location.reload(), 1000);
            } else {
                alert("❌ Simulation failed.");
            }
        });
});