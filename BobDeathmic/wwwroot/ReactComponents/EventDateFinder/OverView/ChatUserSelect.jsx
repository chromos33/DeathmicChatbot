class ChatUserSelect extends React.Component {
    constructor(props) {
        super(props);
        this.state = { chatUsers: [this.props.chatUsers] };
    }
    render() {
        chatUserNodes = "";
        if (this.state.chatUsers !== undefined) {
            chatUserNodes = this.state.chatUsers[0].map(function (chatUser) {
                return <option key={chatUser.name} value={chatUser.name}>{chatUser.name}</option>
            });
        }
        return (
            <select key={this.props.key} className={"chatUser_"+this.props.key}>
                {chatUserNodes}
            </select>
        );
    }
}