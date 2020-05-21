class StreamSubColumn extends React.Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
        this.state = {Status: props.data.Status};
    }
    handleClick(e) {
        var request = new XMLHttpRequest();
        request.open("POST", "/User/ChangeSubscription",true);
        var curthis = this;
        request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        request.onreadystatechange = function () { // Call a function when the state changes.
            if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                if (this.response === "true") {
                    curthis.setState({ Status: true });
                }
                else {
                    curthis.setState({ Status: false });
                }
            }
        }
        request.send("Id="+this.props.data.SubID);
    }
    render() {
        if (this.state.Status) {
            return (<td className="text-center">
                <i onClick={this.handleClick} className="fa fa-power-off font_green pointer"></i>
                </td>
                );
        }
        else {
            return <td className="text-center"><i onClick={this.handleClick} className="fa fa-power-off font_red pointer"></i></td>;
        }
    }
}