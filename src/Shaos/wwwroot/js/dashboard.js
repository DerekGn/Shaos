"use strict";

const eventSource = new EventSource('/api/v1/events/streamparameterevents');
const elementCount = 3;

window.onload = function () {

    eventSource.addEventListener('parameter-updated-event-bool', (event) => {
        console.debug('parameter-updated-event-bool');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[1].children[0].children[0].checked = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();
        }
    });

    eventSource.addEventListener('parameter-updated-event-float', (event) => {
        console.debug('parameter-updated-event-float');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();

            if (parameter.canWrite) {
                element.children[1].children[0].value = parameter.value;
            }
        }
    });

    eventSource.addEventListener('parameter-updated-event-int', (event) => {
        console.debug('parameter-updated-event-int');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();

            if (parameter.canWrite) {
                element.children[1].children[0].value = parameter.value;
            }
        }
    });

    eventSource.addEventListener('parameter-updated-event-string', (event) => {
        console.debug('parameter-updated-event-string');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        if (element != null && element.children.length == elementCount) {

            var startIndex = 0;

            if (element.canWrite) {
                startIndex = 1;
            }

            element.children[startIndex].innerHTML = parameter.value;
            element.children[startIndex+1].innerHTML = new Date(parameter.timestamp).toLocaleString();
        }
    });

    eventSource.addEventListener('parameter-updated-event-uint', (event) => {
        console.debug('parameter-updated-event-uint');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.id);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();

            if (parameter.canWrite) {
                element.children[1].children[0].value = parameter.value;
            }
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
