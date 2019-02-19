class InvitedUserList extends React.Component {
    constructor(props) {
        super(props);
        this.state = { InvitedUsers: [] };
        this.handleAddedChatMember = this.handleAddedChatMember.bind(this);
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
        this.props.eventEmitter.addListener("AddedChatMember", thisreference.handleAddedChatMember);
    }
    handleAddedChatMember(event) {
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
    handleOnClick(event) {
        
    }
    handleOnChange(event) {
    }
    render() {
        chatUserNodes = "";
        if (this.state.InvitedUsers.length > 0) {
            console.log(this.state.InvitedUsers)
            chatUserNodes = this.state.InvitedUsers.map(function (chatUser) {
                return (<div key={chatUser.key} className="col-12 userListItem">
                    <span>{chatUser.name}</span><span className="button">remove</span>
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