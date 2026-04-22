"use strict";

const eventSource = new EventSource('/api/v1/events');

const settings = JSON.parse(document.getElementById('settings').innerHTML);
const ctx = document.getElementById('chartCanvas').getContext('2d');
const chart = new Chart(ctx, {
    type: 'line',
    data: {
        datasets: [
            {
                label: settings.label,
                data: []
            }
        ]
    },
    options: {
        scales: {
            xAxes: [{
                type: 'realtime',
                delay: 0,
                duration: 20000
            }],
            yAxes: [{
                ticks: {
                    suggestedMin: 0,
                    suggestedMax: 50
                }
            }]
        }
    }
});

window.onload = function () {

    eventSource.addEventListener('parameter-updated-event-bool', (event) => {
        console.debug('parameter-updated-event-bool');

        const parameter = JSON.parse(event.data);
    });

    eventSource.addEventListener('parameter-updated-event-float', (event) => {
        console.debug('parameter-updated-event-float');

        const parameter = JSON.parse(event.data);
    });

    eventSource.addEventListener('parameter-updated-event-int', (event) => {
        console.debug('parameter-updated-event-int');

        const parameter = JSON.parse(event.data);
    });

    eventSource.addEventListener('parameter-updated-event-string', (event) => {
        console.debug('parameter-updated-event-string');

        const parameter = JSON.parse(event.data);
    });

    eventSource.addEventListener('parameter-updated-event-uint', (event) => {
        console.debug('parameter-updated-event-uint');

        const parameter = JSON.parse(event.data);
    });

    eventSource.onerror = (err) => {
        console.error('EventSource failed:', err);
        eventSource.close();
    };

    eventSource.onopen = () => {
        console.log('Connection to server opened.');
    };
}