window.TimelineWrapper = {
    create: function (componentName) {
        // DOM element where the Timeline will be attached
        var container = document.getElementById(componentName ? componentName : 'timeline-visualization');
        console.log("----- container", container)
        var groups = new vis.DataSet([
            { id: 1, content: 'Phase&nbsp;1' },
            { id: 2, content: 'Phase&nbsp;2' },
            { id: 3, content: 'Phase&nbsp;3' },
            { id: 4, content: 'Phase&nbsp;4' }
        ]);

        var items = new vis.DataSet();

        var start = new Date('2015-09-16');
        var end = new Date('2015-09-25');
        items.add({
            id: 1,
            group: 1,
            start: start,
            end: end,
            className: 'p1',
            content: 'Project 1'
        });

        start = new Date('2015-09-25');
        end = new Date('2015-10-05');
        items.add({
            id: 2,
            group: 2,
            start: start,
            end: end,
            className: 'p1',
            content: 'Project 1'
        });

        start = new Date('2015-10-01');
        end = new Date('2015-11-15');
        items.add({
            id: 3,
            group: 3,
            start: start,
            end: end,
            className: 'p1',
            content: 'Project 1'
        });

        start = new Date('2015-11-15');
        end = new Date('2015-12-25');
        items.add({
            id: 4,
            group: 4,
            start: start,
            end: end,
            className: 'p1',
            content: 'Project 1'
        });

        start = new Date('2015-09-25');
        end = new Date('2015-10-25');
        items.add({
            id: 5,
            group: 1,
            start: start,
            end: end,
            className: 'p2',
            content: 'Project 2'

        });

        var options = {
            stack: false,
            min: new Date('2015-01-01'),                // lower limit of visible range
            max: new Date('2015-12-31'),                // upper limit of visible range
            //hiddenDates: [
            //    { start: '2013-10-26 00:00:00', end: '2013-10-28 00:00:00', repeat: 'weekly' } // hide weekends
            //],
            //zoomMin: 1000 * 60 * 60 * 24 * 7,             // one week in milliseconds
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