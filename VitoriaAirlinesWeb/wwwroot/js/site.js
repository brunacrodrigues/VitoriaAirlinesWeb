//// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
//// for details on configuring this project to bundle and minify static web assets.

//// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", () => {
    const table = document.querySelector("#flightTable");
    if (!table || !window.flightConnection) return;

    const connection = window.flightConnection;

    connection.on("NewFlightScheduled", function (flight) {
        console.log("NewFlightScheduled:", flight);

        const tbody = table.querySelector("tbody");

        const newRow = document.createElement("tr");
        newRow.setAttribute("data-flight-id", flight.id);
        newRow.setAttribute("data-departure-iso", flight.departureIso);
        newRow.innerHTML = `
            <td>${flight.flightNumber}</td>
            <td>${flight.airplaneModel}</td>
            <td><img src="${flight.originCountryFlagUrl}" width="30" class="me-2" />${flight.originAirportFullName}</td>
            <td><img src="${flight.destinationCountryFlagUrl}" width="30" class="me-2" />${flight.destinationAirportFullName}</td>
            <td>${flight.departureFormatted}</td>
            <td class="status-cell"><span class="badge dashboard-badge-warning"><i class="fas fa-clock me-1"></i> Scheduled</span></td>
        `;

        const departureDate = new Date(flight.departureIso);
        const rows = Array.from(tbody.querySelectorAll("tr[data-departure-iso]"));
        let inserted = false;

        for (let i = 0; i < rows.length; i++) {
            const rowDate = new Date(rows[i].dataset.departureIso);
            if (departureDate < rowDate) {
                tbody.insertBefore(newRow, rows[i]);
                inserted = true;
                break;
            }
        }

        if (!inserted) tbody.appendChild(newRow);

        const emptyRow = tbody.querySelector("td[colspan='5']");
        if (emptyRow) emptyRow.parentElement.remove();

        newRow.classList.add("highlighted");
        setTimeout(() => newRow.classList.remove("highlighted"), 1000);
    });

    connection.on("FlightStatusChanged", function (flightId, newStatus) {
        const row = table.querySelector(`[data-flight-id="${flightId}"]`);
        const statusCell = row?.querySelector(".status-cell");
        if (!row || !statusCell) return;

        const badgeHtml = {
            "Departed": '<span class="badge dashboard-badge-primary"><i class="fas fa-plane-departure me-1"></i> Departed</span>',
            "Completed": '<span class="badge dashboard-badge-success"><i class="fas fa-plane-arrival me-1"></i> Completed</span>',
            "Canceled": '<span class="badge dashboard-badge-danger"><i class="fas fa-times-circle me-1"></i> Canceled</span>'
        };

        if (badgeHtml[newStatus]) {
            statusCell.innerHTML = badgeHtml[newStatus];
        }

        if (newStatus !== "Scheduled") {
            row.classList.add("fade-out");
            setTimeout(() => row.remove(), 1000);
        }
    });

    connection.on("UpdatedFlight", function (flight) {
        const row = table.querySelector(`tr[data-flight-id="${flight.id}"]`);
        if (!row) return;

        row.setAttribute("data-departure-iso", flight.departureIso);
        row.innerHTML = `
            <td>${flight.flightNumber}</td>
            <td>${flight.airplaneModel}</td>
            <td><img src="${flight.originCountryFlagUrl}" width="30" class="me-2" />${flight.originAirportFullName}</td>
            <td><img src="${flight.destinationCountryFlagUrl}" width="30" class="me-2" />${flight.destinationAirportFullName}</td>
            <td>${flight.departureFormatted}</td>
            <td class="status-cell"><span class="badge dashboard-badge-warning"><i class="fas fa-clock me-1"></i> Scheduled</span></td>
        `;

        // Reorder the row if necessary
        const tbody = table.querySelector("tbody");
        const departureDate = new Date(flight.departureIso);
        const rows = Array.from(tbody.querySelectorAll("tr[data-departure-iso]")).filter(r => r !== row);

        let inserted = false;
        for (let i = 0; i < rows.length; i++) {
            const rowDate = new Date(rows[i].dataset.departureIso);
            if (departureDate < rowDate) {
                tbody.insertBefore(row, rows[i]);
                inserted = true;
                break;
            }
        }

        if (!inserted) tbody.appendChild(row);

        row.classList.add("highlighted");
        setTimeout(() => row.classList.remove("highlighted"), 1000);
    });

    console.log("Flight dashboard listeners active");
});
