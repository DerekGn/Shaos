"use strict";

const options = JSON.parse(document.getElementById('options').innerHTML);
const settings = JSON.parse(document.getElementById('settings').innerHTML);
const eventSource = new EventSource('/api/v1/parameter-events/' + settings.id);
const context = document.getElementById('chart');
const smoothie = new SmoothieChart(
    {
        interpolation: settings.interpolation.toLowerCase(),
        grid:
        {
            fillStyle: options.gridSettings.fillColour,
            lineWidth: options.gridSettings.lineWidth,
            strokeStyle: options.gridSettings.lineColour,
            verticalSections: options.gridSettings.verticalSections,
            borderVisible: options.gridSettings.borderVisible
        },
        labels:
        {
            disabled: options.labelSettings.disabled,
            fillStyle: options.labelSettings.colour,
            fontSize: options.labelSettings.fontSize,
            fontFamily: options.labelSettings.fontFamily,
            precision: options.labelSettings.precision,
            showIntermediateLabels: options.labelSettings.showIntermediateLabels,
            intermediateLabelSameAxis: options.labelSettings.intermediateLabelSameAxis
        },
        // title:
        // {
        //     text: settings.label,                   // the text to display on the left side of the chart
        //     fillStyle: '#ffffff',                   // colour for text
        //     fontSize: 10,
        //     fontFamily: 'sans-serif',
        //     verticalAlign: 'top'                    // one of 'top', 'middle', or 'bottom'
        // },

        // //minValue: 0,                                 // specify to clamp the lower y-axis to a given value
        // //maxValue: 1,                                  // specify to clamp the upper y-axis to a given value
        // maxValueScale: 1.1,                             // allows proportional padding to be added above the chart. for 10% padding, specify 1.1.
        // minValueScale: 1.1,                             // allows proportional padding to be added below the chart. for 10% padding, specify 1.1.
        // //yRangeFunction: undefined,                    // function({min: , max: }) { return {min: , max: }; }
        // scaleSmoothing: 0.125,                          // controls the rate at which y-value zoom animation occurs
        // millisPerPixel: 20,                             // sets the speed at which the chart pans by
        // enableDpiScaling: true,                         // support rendering at different DPI depending on the device
        // // yMinFormatter: function(min, precision) { // callback function that formats the min y value label
        // //     return parseFloat(min).toFixed(precision);
        // // },
        // // yMaxFormatter: function(max, precision) { // callback function that formats the max y value label
        // //     return parseFloat(max).toFixed(precision);
        // // },
        // // yIntermediateFormatter: function(intermediate, precision) { // callback function that formats the intermediate y value labels
        // //     return parseFloat(intermediate).toFixed(precision);
        // // },
        // maxDataSetLength: 2,
        // interpolation: 'step',                      // one of 'bezier', 'linear', or 'step'
        // //timestampFormatter: SmoothieChart.timeFormatter,               // optional function to format time stamps for bottom of chart
        //                                             // you may use SmoothieChart.timeFormatter, or your own: function(date) { return ''; }
        // scrollBackwards: false,
        // //horizontalLines: [{ value: 0, color: '#c0c0c0', lineWidth: 1 }, { value: 1, color: '#c0c0c0', lineWidth: 1 }],
        // grid:
        // {
        //     fillStyle: '#ffffff',                   // the background colour of the chart
        //     lineWidth: 1,                           // the pixel width of grid lines
        //     strokeStyle: '#c0c0c0',                 // colour of grid lines
        //     millisPerLine: 1000,                    // distance between vertical grid lines
        //     verticalSections: 0,                    // number of vertical sections marked out by horizontal grid lines
        //     borderVisible: true                     // whether the grid lines trace the border of the chart or not
        // },
        // labels:
        // {
        //     disabled: true,                         // enables/disables labels showing the min/max values
        //     fillStyle: '#ffffff',                   // colour for text of labels,
        //     fontSize: 15,
        //     fontFamily: 'sans-serif',
        //     precision: 2,
        //     showIntermediateLabels: false,          // shows intermediate labels between min and max values along y axis
        //     intermediateLabelSameAxis: true,
        // },
        // title:
        // {
        //     text: settings.label,                   // the text to display on the left side of the chart
        //     fillStyle: '#ffffff',                   // colour for text
        //     fontSize: 10,
        //     fontFamily: 'sans-serif',
        //     verticalAlign: 'top'                    // one of 'top', 'middle', or 'bottom'
        // },
        tooltip: true,                                  // show tooltip when mouse is over the chart
        tooltipLine: {                                  // properties for a vertical line at the cursor position
            lineWidth: 1,
            strokeStyle: '#BBBBBB'
        },
        // tooltipFormatter: SmoothieChart.tooltipFormatter,   // formatter function for tooltip text
        // nonRealtimeData: false,                             // use time of latest data as current time
        // displayDataFromPercentile: 1,                       // display not latest data, but data from the given percentile
        // useful when trying to see old data saved by setting a high value for maxDataSetLength
        // should be a value between 0 and 1
        responsive: true,                           // whether the chart should adapt to the size of the canvas
        //limitFPS: 0                                 // maximum frame rate the chart will render at, in FPS (zero means no limit)
    });

const line = new TimeSeries();

smoothie.streamTo(context);

smoothie.addTimeSeries(line,
    {
        lineWidth: 2,
        strokeStyle: '#0080ff',
        fillStyle: 'rgba(0,128,255,0.30)',
        //interpolation: undefined;
        //tooltipLabel: undefined,
        fillToBottom: false,
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