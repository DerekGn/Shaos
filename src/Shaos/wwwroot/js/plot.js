"use strict";

const options = JSON.parse(document.getElementById('options').innerHTML);
const settings = JSON.parse(document.getElementById('settings').innerHTML);
const eventSource = new EventSource('/api/v1/parameter-events/' + settings.id);
const context = document.getElementById('chart');
const smoothie = new SmoothieChart(
    {
        interpolation: settings.interpolation.toLowerCase(),
        grid: {
            fillStyle: options.gridSettings.fillColour,
            lineWidth: options.gridSettings.lineWidth,
            strokeStyle: options.gridSettings.lineColour,
            verticalSections: options.gridSettings.verticalSections,
            borderVisible: options.gridSettings.borderVisible
        },
        labels: {
            disabled: options.labelSettings.disabled,
            fillStyle: options.labelSettings.colour,
            fontSize: options.labelSettings.fontSize,
            fontFamily: options.labelSettings.fontFamily,
            precision: options.labelSettings.precision,
            showIntermediateLabels: options.labelSettings.showIntermediateLabels,
            intermediateLabelSameAxis: options.labelSettings.intermediateLabelSameAxis
        },
        title: {
            text: settings.label,
            fillStyle: options.titleSettings.colour,
            fontSize: options.titleSettings.fontSize,
            fontFamily: options.titleSettings.fontFamily,
            verticalAlign: options.titleSettings.alignment
        },
        tooltip: options.toolTipSettings.enabled,
        tooltipLine: {
            lineWidth: options.toolTipSettings.lineWidth,
            strokeStyle: options.toolTipSettings.lineColour
        },
        responsive: true
    });

const line = new TimeSeries();

smoothie.streamTo(context);

smoothie.addTimeSeries(line,
    {
        lineWidth: options.timeSeriesSettings.lineWidth,
        strokeStyle: options.timeSeriesSettings.lineColour,
        fillStyle: options.timeSeriesSettings.fillColour,
        //interpolation: undefined;
        tooltipLabel: settings.timeSeriesLabel,
        fillToBottom: options.timeSeriesSettings.fillToBottom
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

        console.debug('Value: ' + value);

        line.append(Date.parse(timestamp), value);
    }
}