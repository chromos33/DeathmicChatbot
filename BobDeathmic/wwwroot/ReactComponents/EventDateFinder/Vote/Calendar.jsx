class Calendar extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [], eventEmitter: new EventEmitter() };
    }
    componentWillMount() {
        var thisreference = this;
        const xhr = new XMLHttpRequest();
        console.log("/Events/GetEventDates/" + this.props.ID);
        xhr.open('GET', "/Events/GetEventDates/" + this.props.ID, true);
        xhr.onload = function () {
            thisreference.setState({ data: JSON.parse(xhr.responseText) });
        };
        xhr.send();
    }
    render() {
        if (this.state.data.Header !== undefined && this.state.data.Header.length > 0) {
            var tempthis = this;
            headerNodes = this.state.data.Header.map(function (Header) {
                return <EventDate key={Header.Date + Header.Time} Data={Header}/>;
            });
            return (
                <div className="EventDateContainer">
                    {headerNodes}
                </div>
            );
        }
        else {
            return <span>Loading</span>;
        }
        
    }
}
