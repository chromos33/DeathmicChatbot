class ChatUserSelect extends React.Component {
    constructor(props) {
        super(props);
        this.state = { chatUsers: [], selectedUser: "" };
        this.handleOnClick = this.handleOnClick.bind(this);
        this.handleOnChange = this.handleOnChange.bind(this);
    }
    componentWillMount() {
        var thisreference = this;
        $.ajax({
            url: "/EventDateFinder/InvitableUsers/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                thisreference.setState({ chatUsers: result, selectedUser: result[0] });
            }
        });
    }
    handleOnClick(event) {
        var thisreference = this;
        $.ajax({
            url: "/EventDateFinder/AddInvitedUser/",
            type: "POST",
            data: {
                ID: thisreference.props.ID,
                ChatUser: thisreference.state.selectedUser.name
            },
            success: function (result) {
                thisreference.props.eventEmitter.emitEvent("UpdateChatMembers");
            }
        });
    }
    handleOnChange(event) {
        this.setState({ selectedUser: event.target.value});
    }
    render() {
        chatUserNodes = "";
        if (this.state.chatUsers.length > 0) {
            chatUserNodes = this.state.chatUsers.map(function (chatUser) {
                return <option key={chatUser.name} value={chatUser.name}>{chatUser.name}</option>
            });
            return (
                <div>
                    <select key={this.props.key} value={this.state.selectedUser} onChange={this.handleOnChange} className={"chatUser_" + this.props.key}>
                        {chatUserNodes}
                    </select>
                    <span className="button" onClick={this.handleOnClick}>Invite</span>
                </div>
            );
        }
        return <p> No Users Loaded</p>;
        
    }
}