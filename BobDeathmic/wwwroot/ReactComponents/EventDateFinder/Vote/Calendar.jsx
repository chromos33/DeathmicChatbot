class Calendar extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [], eventEmitter: new EventEmitter() };
    }
    componentWillMount() {
        var thisreference = this;
        $.ajax({
            url: "/EventDateFinder/GetEventDates/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                console.log(result);
                thisreference.setState({ data: result });
            }
        });
    }
    render() {
        if (this.state.data.Header !== undefined && this.state.data.Header.length > 0 && this.state.data.User !== undefined) {
            var tempthis = this;
            headerNodes = this.state.data.Header.map(function (Header) {
                return <span key={Header.Date + Header.Time} className="voteHeader"><span className="date">{Header.Date}</span><br /><span className="time">{Header.Time}</span></span>;
            });
            userNodes = this.state.data.User.map(function (User) {
                return <VoteMember key={User.Name} Name={User.Name} canEdit={User.canEdit} Requests={User.Requests} />;
            });
            return (
                <div>
                    <div className="headerNodes">
                        <span className="voteHeader">&nbsp;</span>
                        {headerNodes}
                    </div>
                    {userNodes}
                </div>
            );
        }
        else {
            return <span>Loading</span>;
        }
        
    }
}
