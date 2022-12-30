let timeline;
window.TimelineWrapper = {
    create: function (componentName, users, leaveRequests, minDate, maxDate) {
        // DOM element where the Timeline will be attached
        if (timeline) {
            timeline.destroy();
        }
        const container = document.getElementById(componentName ? componentName : 'timeline-visualization');

        const groups = new vis.DataSet(users.map((u) => ({ id: u.id, content: u.name })));
        const items = new vis.DataSet();
        leaveRequests.forEach(lr => {
            items.add({
                id: lr.id,
                group: lr.createdBy.id,
                start: new Date(lr.dateFrom).setHours(0, 0, 0, 0),
                end: new Date(lr.dateTo).setHours(23, 59, 59, 99),
                content: lr.leaveTypeId
            });
        });

        const options = {
            stack: false,
            editable: false,
            margin: {
                item: 10, // minimal margin between items
                axis: 5   // minimal margin between items and the axis
            },
            orientation: 'top',
            timeAxis: { scale: 'day' },
            min: minDate,                // lower limit of visible range
            max: maxDate,                // upper limit of visible range

        };
        timeline = new vis.Timeline(container, null, options);
        timeline.setGroups(groups);
        timeline.setItems(items);
        container.onclick = function (event) {
            var props = timeline.getEventProperties(event);
            if (props.item) {
                window.location.href = `/leave-requests/${props.item}`;
            }
        }
    }
}