class LinkColumn extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return <td><a href={this.props.data.Link}>{this.props.data.Text}</a></td>;

    }
}
