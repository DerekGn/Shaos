"use strict";

const settings = JSON.parse(document.getElementById('settings').innerHTML);
const eventSource = new EventSource('/api/v1/parameter-events/' + settings.id);
const container = document.getElementById('container')

var data = [];

const graph = new Dygraph(container, data,
    {
        drawPoints: true,
        showRoller: true,
        stepPlot: true,
        fillGraph: true,
        valueRange: [0.0, 1],
        labels: ['Time', 'State']
    });

window.onload = function () {

    eventSource.addEventListener('parameter-updated-event-bool', (event) => {
        console.debug('parameter-updated-event-bool');

        const parameter = JSON.parse(event.data);

        plotParameterValue(parameter.value ? 1 : 0, parameter.timestamp);
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

        data.push([new Date(timestamp), value]);

        if (data.length > 200) {
            data.shift();
        }

        graph.updateOptions({ 'file': data });
    }
}