class Calendar extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return (
            <div className="row">
                <div className="col-md-6 col-12 mb-4">
                    {this.props.name}
                </div>
                <div className="col-md-6 col-12 mb-4">
                    <a className="button" href={this.props.editLink}>Edit</a>
                </div>
                <div className="col-md-6 col-12 mb-4">
                    <ChatUserSelect chatUsers={this.props.chatUsers} />
                </div>
                <div className="col-md-6 col-12 mb-4">
                    <span className="button">Invite</span>
                </div>
            </div>
        );
    }
}