"use strict";

const settings = JSON.parse(document.getElementById('settings').innerHTML);
const ctx = document.getElementById('chartCanvas').getContext('2d');
const plot = new Chart(ctx, {
    type: 'line',
    data: {
        datasets: [
            {
                label: settings.label
            }
        ]
    }
});


var connection = new signalR
    .HubConnectionBuilder()
    .withUrl("/plotHub")
    .build();

async function start() {
    try {
        await connection.start();

        //connection.invoke("Start", settings.id).catch(function (err) {
        //    return console.error(err.toString());
        //});

        console.log("SignalR Connected.");
        console.log(document.getElementById('settings').innerHTML);
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

connection.onclose(async () => {
    await start();
});

connection.on("update", function (point) {

    plot.data.labels.push(point.label);
    plot.data.datasets.forEach((dataset) => {
        dataset.data.push(point.value);
    });

    plot.update();

    if (plot.data.labels.length > data.limit) {
        plot.data.labels.splice(0, 1);
        plot.data.datasets.forEach((dataset) => {
            dataset.data.splice(0, 1);
        });
        plot.update();
    }
});

start().then(() => { });