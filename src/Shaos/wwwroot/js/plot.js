const settings = JSON.parse(document.getElementById('settings').innerHTML);
const eventSource = new EventSource('/api/v1/events/streamparameterevents/' + settings.id);
const ctx = document.getElementById('chartCanvas').getContext('2d');
const chart = new Chart(ctx, {
    type: 'line',
    data: {
        datasets: [{
            fill: true,
            label: settings.label,
            data: [],
            tension: 0.1
        }]
    },
    options: {
        scales: {
            x: {
                type: 'realtime',
                delay: 0,
                duration: settings.duration
            },
            y: {
                ticks: {
                    suggestedMin: 0,
                    suggestedMax: 50
                }
            }
        }
    }
});

window.onload = function () {

    eventSource.addEventListener('parameter-updated-event-bool', (event) => {
        console.debug('parameter-updated-event-bool');

        const parameter = JSON.parse(event.data);

        plotParameterValue(parameter.value, parameter.timestamp);
    });

    eventSource.addEventListener('parameter-updated-event-float', (event) => {
        console.debug('parameter-updated-event-float');

        const parameter = JSON.parse(event.data);

        plotParameterValue(parameter.value, parameter.timestamp);
    });

    eventSource.addEventListener('parameter-updated-event-int', (event) => {
        console.debug('parameter-updated-event-int');

        const parameter = JSON.parse(event.data);

        plotParameterValue(parameter.value, parameter.timestamp);
    });

    eventSource.addEventListener('parameter-updated-event-string', (event) => {
        console.debug('parameter-updated-event-string');

        const parameter = JSON.parse(event.data);

        plotParameterValue(parameter.value, parameter.timestamp);
    });

    eventSource.addEventListener('parameter-updated-event-uint', (event) => {
        console.debug('parameter-updated-event-uint');

        const parameter = JSON.parse(event.data);

        plotParameterValue(parameter.value, parameter.timestamp);
    });

    eventSource.onerror = (err) => {
        console.error('EventSource failed:', err);
        eventSource.close();
    };

    eventSource.onopen = () => {
        console.log('Connection to server opened.');
    };

    function plotParameterValue(value, timestamp) {
        var dateValue = moment(timestamp).subtract(5, 'seconds');

        chart.data.datasets[0].data.push({
            x: timestamp,
            y: value
        });

        chart.update({
            preservation: true
        });
    }
}