﻿class OverView extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [] };
    }
    componentWillMount() {

        const xhr = new XMLHttpRequest();
        xhr.open('get', this.props.url, true);
        xhr.onload = () => {
            const data = JSON.parse(xhr.responseText);
            this.setState({ data: data });
        };
        xhr.send();
    }
    render() {
        calendarNodes = "";
        console.log(this.state.data);
        if (this.state.data.calendars !== undefined) {
            calendarNodes = this.state.data.calendars.map(function (calendar) {
                return <OverViewCalendar key={calendar.key} deleteLink={calendar.deleteLink} editLink={calendar.editLink} name={calendar.name} voteLink={calendar.voteLink} chatUsers={calendar.chatUsers}></Calendar>
            });
        }
        return (
            <div className="OverView">
                <a className="button" href={this.state.data.addCalendarLink}>Add Calendar</a><br /><br />
                {calendarNodes}
            </div>
        );
    }
}
ReactDOM.render(<OverView url="/Events/OverViewData" />, document.getElementById('reactcontent'));