class OverView extends React.Component {
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
        if (this.state.data.calendars !== undefined) {
            calendarNodes = this.state.data.calendars.map(function (comment) {
                return <Calendar key={comment.key} editLink={comment.editLink} name={comment.name} voteLink={comment.voteLink} chatUsers={comment.chatUsers}></Calendar>
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
ReactDOM.render(<OverView url="/EventDateFinder/OverViewData" />, document.getElementById('reactcontent'));