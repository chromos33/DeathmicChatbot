class Calendar extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [], eventEmitter: new EventEmitter() };
    }
    componentWillMount() {
        var thisreference = this;
        $.ajax({
            url: "/Events/GetEventDates/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                console.log(result);
                thisreference.setState({ data: result });
            }
        });
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
