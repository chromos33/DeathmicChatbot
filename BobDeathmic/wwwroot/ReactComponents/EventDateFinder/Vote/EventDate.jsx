class EventDate extends React.Component {
    constructor(props) {
        super(props);
        this.state = { Data: this.props.Data };
    }
    render() {
        var tmpthis = this;
        var key = 0;
        var requestnodes = this.state.Data.Requests.map(function (request) {
            key++;
            return (
                <div className="row usernode mr-0 ml-0">
                    <span className="col-6 pt-0 pb-0 pl-0 pr-0">{request.UserName}</span>
                    <StateSelect key={key} canEdit={request.canEdit} requestID={request.AppointmentRequestID} possibleStates={request.States} state={request.State} />
                </div>
                );
        });
        if (requestnodes.length > 0) {
            return (
                <div key={this.key} className="EventDate">
                    <div className="row ml-0 mr-0">
                        <span className="col-12 text-center bg_dark">
                            <span className="date">{this.state.Data.Date}</span><br />
                            <span className="time">{this.state.Data.Time}</span>
                        </span>
                    </div>
                    {requestnodes}
                </div>
            );
        }
        else {
            return (
                <div key={this.key} className="VoteUser">
                    <span className="VoteUser_Name">{this.props.Name}</span>
                </div>
            );
        }
        
    }
}
