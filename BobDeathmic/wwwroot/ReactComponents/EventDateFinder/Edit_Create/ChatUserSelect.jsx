class ChatUserSelect extends React.Component {
    constructor(props) {
        super(props);
        this.state = { chatUsers: [], selectedUser: "" };
        this.handleOnClick = this.handleOnClick.bind(this);
        this.handleOnChange = this.handleOnChange.bind(this);
    }
    async componentWillMount() {
        var thisreference = this;
        var users = await fetch("/Events/InvitableUsers/" + this.props.ID, {
            method: 'GET',
            headers: {
                Accept: 'application/json'
            }
        }).then((response) => {
            return response.json();
        }).then((json) => {
            return JSON.parse(json);
        });
        thisreference.setState({ chatUsers: users, selectedUser: users[0].Name });
    }
    handleOnClick(event) {
        
        var thisreference = this;
        $.ajax({
            url: "/Events/AddInvitedUser/",
            type: "POST",
            data: {
                ID: thisreference.props.ID,
                ChatUser: thisreference.state.selectedUser
            },
            success: function (result) {
                thisreference.props.eventEmitter.emitEvent("UpdateChatMembers");
            }
        });
    }
    handleOnChange(event) {
        console.log(event.target.value);
        this.setState({ selectedUser: event.target.value});
    }
    render() {
        if (this.state.chatUsers.length > 0) {
            var chatUserNodes = this.state.chatUsers.map(function (chatUser) {
                return <option key={chatUser.Name} value={chatUser.Name}>{chatUser.Name}</option>;
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