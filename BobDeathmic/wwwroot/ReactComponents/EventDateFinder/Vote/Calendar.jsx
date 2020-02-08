class Calendar extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [], eventEmitter: new EventEmitter(), mode: "default" };
        this.changeMode = this.changeMode.bind(this);
    }
    componentWillMount() {
        var thisreference = this;
        const xhr = new XMLHttpRequest();
        xhr.open('GET', "/Events/GetEventDates/" + this.props.ID, true);
        xhr.onload = function () {
            thisreference.setState({ data: JSON.parse(xhr.responseText) });
        };
        xhr.send();
    }
    changeMode(event) {
        this.setState({
            mode: event.target.value
        });
    }
    render() {
        if (this.state.data.Header !== undefined && this.state.data.Header.length > 0) {
            var tempthis = this;
            var headerNodes = this.state.data.Header.map(function (Header) {
                return <EventDate mode={tempthis.state.mode} key={Header.Date + Header.Time} Data={Header}/>;
            });
            return (
                <div>
                    <select className="ml-3 mb-5" onChange={this.changeMode}>
                        <option value="default">Standard</option>
                        <option value="fallback">Mobile Fallback</option>
                    </select>
                    <div className="EventDateContainer">
                    {headerNodes}
                    </div>
                </div>
            );
        }
        else {
            return <span>Loading</span>;
        }
        
    }
}
