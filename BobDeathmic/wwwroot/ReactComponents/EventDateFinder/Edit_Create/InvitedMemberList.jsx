class InvitedUserList extends React.Component {
    constructor(props) {
        super(props);
        this.state = { InvitedUsers: [] };
        this.handleUpdateChatMembers = this.handleUpdateChatMembers.bind(this);
        this.handleOnRemoveClick = this.handleOnRemoveClick.bind(this);
    }
    componentWillMount() {
        var thisreference = this;
        $.ajax({
            url: "/EventDateFinder/InvitedUsers/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                thisreference.setState({ InvitedUsers: result });
            }
        });
        this.props.eventEmitter.addListener("UpdateChatMembers", thisreference.handleUpdateChatMembers);
    }
    handleUpdateChatMembers(event) {
        var thisreference = this;
        $.ajax({
            url: "/EventDateFinder/InvitedUsers/" + thisreference.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                thisreference.setState({ InvitedUsers: result });
            }
        });
    }
    handleOnRemoveClick(event) {
        var thisreference = this;
        event.persist();        
        $.ajax({
            url: "/EventDateFinder/RemoveInvitedUser/",
            type: "POST",
            data: {
                ID: thisreference.props.ID,
                ChatUser: event.target.dataset.value
            },
            success: function (result) {
                thisreference.props.eventEmitter.emitEvent("UpdateChatMembers");
            }
        });
    }
    handleOnClick(event) {
        
    }
    handleOnChange(event) {
    }
    render() {
        chatUserNodes = "";
        if (this.state.InvitedUsers.length > 0) {
            var tempthis = this;
            chatUserNodes = this.state.InvitedUsers.map(function (chatUser) {
                return (<div key={chatUser.key} className="col-12 userListItem">
                    <span>{chatUser.name}</span><span onClick={tempthis.handleOnRemoveClick} data-value={chatUser.name} className="button">remove</span>
                </div>);
            });
            return (
                <div>
                    <div className="row" key={this.props.key}>
                        {chatUserNodes}
                    </div>
                </div>
            );
        }
        return <p> No Users Loaded</p>;

    }
}