class CurrentItemInfo extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        let box;
        if (this.props.Game === "") {
            box = "Test";
        }
        else {
            box = this.props.Game;
        }
        return (
                <h2 className="mb-4"><a href={this.props.Link}>{box}</a></h2>
        );
    }
}
