"use strict";

const eventSource = new EventSource('/api/v1/parameter-events');
const elementCount = 4;

window.onload = function () {

    eventSource.addEventListener('parameter-updated-event-bool', (event) => {

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(2);

        console.debug('parameter-updated-event-bool ' + parameter.id);

        if (element != null && element.children.length == elementCount) {

            element.querySelector("#value").innerHTML = parameter.value;
            element.querySelector("#input").checked = parameter.value;
            element.querySelector("#timestamp").innerHTML = new Date(parameter.timestamp).toLocaleString();
        }
    });

    eventSource.addEventListener('parameter-updated-event-float', (event) => {

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        console.debug('parameter-updated-event-float ' + parameter.id);

        if (element != null && element.children.length == elementCount) {

            element.querySelector("#value").innerHTML = parameter.value;
            element.querySelector("#timestamp").innerHTML = new Date(parameter.timestamp).toLocaleString();

            if (parameter.canWrite) {
                element.querySelector("#input").checked = parameter.value;
            }
        }
    });

    eventSource.addEventListener('parameter-updated-event-int', (event) => {

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        console.debug('parameter-updated-event-int ' + parameter.id);

        element.querySelector("#value").innerHTML = parameter.value;
        element.querySelector("#timestamp").innerHTML = new Date(parameter.timestamp).toLocaleString();

        if (parameter.canWrite) {
            element.querySelector("#input").checked = parameter.value;
        }
    });

    eventSource.addEventListener('parameter-updated-event-string', (event) => {

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        console.debug('parameter-updated-event-string ' + parameter.id);

        if (element != null && element.children.length == elementCount) {

            let startIndex = 0;

            if (element.canWrite) {
                startIndex = 1;
            }

            element.children[startIndex].innerHTML = parameter.value;
            element.children[startIndex+1].innerHTML = new Date(parameter.timestamp).toLocaleString();
        }
    });

    eventSource.addEventListener('parameter-updated-event-uint', (event) => {

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        console.debug('parameter-updated-event-uint ' + parameter.id);

        element.querySelector("#value").innerHTML = parameter.value;
        element.querySelector("#timestamp").innerHTML = new Date(parameter.timestamp).toLocaleString();

        if (parameter.canWrite) {
            element.querySelector("#input").checked = parameter.value;
        }
    });

    eventSource.onerror = (err) => {
        console.error('EventSource failed:', err);
        eventSource.close();
    };

    eventSource.onopen = () => {
        console.log('Connection to server opened.');
    };
}
