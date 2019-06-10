class OverViewCalendar extends React.Component {
    constructor(props) {
        super(props);
        console.log(props);
    }
    render() {
        
        if (this.props.editLink !== "") {
            return (
                <div className="row">
                    <div className="col-md-3 col-12 mb-4">
                        {this.props.name}
                    </div>
                    <div className="col-md-3 col-6 mb-4">
                        <a className="button" href={this.props.voteLink}>Vote</a>
                    </div>
                    <div className="col-md-3 col-6 mb-4">
                        <a className="button" href={this.props.editLink}>Edit</a>
                    </div>
                    <div className="col-md-3 col-6 mb-4">
                        <a className="button" href={this.props.deleteLink}>Delete</a>
                    </div>
                </div>
            );
        }
        else {
            return (
                <div className="row">
                    <div className="col-md-6 col-12 mb-4">
                        {this.props.name}
                    </div>
                    <div className="col-md-3 col-6 mb-4">
                        <a className="button" href={this.props.voteLink}>Vote</a>
                    </div>
                </div>
            );
        }
        
    }
}