class StreamEditColumn extends React.Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
    }
    handleClick(e) {
        //TODO get Edit Data and render static box with fields
    }
    render() {
        return <td>
            {this.props.data.Text}
            <div className="shadowlayer"></div>
            <div className="statictest">
                <span>test</span>
            </div>
        </td>;

    }
}
