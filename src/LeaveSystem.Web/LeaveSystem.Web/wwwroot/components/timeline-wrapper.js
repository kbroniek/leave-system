window.TimelineWrapper = {
    create: function (componentName, users, leaveRequests) {
        // DOM element where the Timeline will be attached
        var container = document.getElementById(componentName ? componentName : 'timeline-visualization');
        console.log("----- users", users)
        console.log("----- leaveRequests", leaveRequests)

        var groups = new vis.DataSet(users.map((u) => ({ id: u.email, content: u.name })));
        var items = new vis.DataSet();
        leaveRequests.forEach(lr => {
            items.add({
                id: lr.id,
                group: lr.createdBy.email,
                start: new Date(lr.dateFrom).setHours(0, 0, 0, 0),
                end: new Date(lr.dateTo).setHours(23, 59, 59, 99),
                content: lr.leaveTypeId
            });
        });

        var options = {
            stack: false,
            editable: false,
            margin: {
                item: 10, // minimal margin between items
                axis: 5   // minimal margin between items and the axis
            },
            orientation: 'top',
            timeAxis: { scale: 'day' }

        };
        timeline = new vis.Timeline(container, null, options);
        timeline.setGroups(groups);
        timeline.setItems(items);
    }
}