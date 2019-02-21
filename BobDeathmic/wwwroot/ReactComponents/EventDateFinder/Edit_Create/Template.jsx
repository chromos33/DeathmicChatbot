class Template extends React.Component {
    constructor(props) {
        super(props);
        this.state = { day: props.day, start: props.start, stop: props.stop, name: props.name };
        this.handleOnDaySelect = this.handleOnDaySelect.bind(this);
        this.handleOnEndChange = this.handleOnEndChange.bind(this);
        this.handleOnStartChange = this.handleOnStartChange.bind(this);
    }
    handleOnDaySelect(event) {
        var tempthis = this;
        event.persist();
        var tempevent = event;
        $.ajax({
            url: "/EventDateFinder/SetDayOfTemplate/" + this.state.name,
            type: "GET",
            data: {
                Day: event.target.value
            },
            success: function (result) {
                if (result === true) {
                    var day = parseInt(tempevent.target.value);
                    tempthis.setState({ day: day });
                }
            }
        });
    }
    handleOnStartChange(event) {
        var tempthis = this;
        event.persist();
        var value = event.target.value;
        $.ajax({
            url: "/EventDateFinder/SetStartOfTemplate/" + this.state.name,
            type: "GET",
            data: {
                Start: event.target.value
            },
            success: function (result) {
                if (result === true) {
                    tempthis.setState({ start: value });
                }
            }
        });
    }
    handleOnEndChange(event) {

    }
    render() {
        var tempthis = this;
        var Days = [{ Day: "Montag", Value: 1 }, { Day: "Dienstag", Value: 2 }, { Day: "Mittwoch", Value: 3 }, { Day: "Donnerstag", Value: 4 }, { Day: "Freitag", Value: 5 }, { Day: "Samstag", Value: 6 }, { Day: "Sonntag", Value: 7 }];
        templateNodes = Days.map(function (Day) {
            return (<div key={Day.Day + Day.Value}><input type="radio" onChange={tempthis.handleOnDaySelect} checked={tempthis.state.day === Day.Value} name={tempthis.state.name} value={Day.Value} /> <span>{Day.Day}</span></div>);
        });
        return (
            <div>
                {templateNodes}
                <input type="time" onChange={this.handleOnStartChange} name="Start" value={this.state.start}/>Start
                <input type="time" onChange={this.handleOnEndChange} name="Stop" value={this.state.stop}/>Ende
            </div>
            );

    }
}