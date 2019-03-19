class VoteMember extends React.Component {
    constructor(props) {
        super(props);
        this.state = { Requests: this.props.Requests };
    }
    render() {
        var tmpthis = this;
        var key = 0;
        var requestnodes = this.state.Requests.map(function (request) {
            key++;
            console.log(request.State);
            return (
                    <StateSelect key={key} canEdit={tmpthis.props.canEdit} requestID={request.AppointmentRequestID} possibleStates={request.States} state={request.State} />
                
                );
        });
        if (requestnodes.length > 0) {
            return (
                <div key={this.key} className="VoteUser">
                    <span className="VoteUser_Name">{this.props.Name}</span>
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
