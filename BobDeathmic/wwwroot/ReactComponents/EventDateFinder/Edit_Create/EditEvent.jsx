class EditEvent extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [], eventEmitter: new EventEmitter()};
    }
    componentWillMount() {
        var thisreference = this;
        $.ajax({
            url: "/Events/GetEvent/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                thisreference.setState({ data: result });
            }
        }); 
    }
    render() {
        if (this.state.data.name === undefined) {
            return (
                <div className="OverView">
                    <NameField owner={this.props.ID} value="" />
                </div>
            );
        }
        else {
            return (
                <div className="OverView">
                    <span>Name</span>
                    <NameField owner={this.props.ID} value={this.state.data.name} />
                    <ChatUserSelect ID={this.props.ID} eventEmitter={this.state.eventEmitter} />
                    <InvitedUserList ID={this.props.ID} eventEmitter={this.state.eventEmitter} />
                    <TemplateList ID={this.props.ID} eventEmitter={this.state.eventEmitter} />
                </div>
            );
        }
        
    }
}
