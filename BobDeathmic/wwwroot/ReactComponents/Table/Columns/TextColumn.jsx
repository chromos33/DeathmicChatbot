class TextColumn extends React.Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
    }
    handleClick(e) {
        this.props.Sort(this.props.id);
    }
    render() {
        if (this.props.data.canSort) {
            return <td onClick={this.handleClick}>{this.props.data.Text}</td>;
        }
        else {
            return <td>{this.props.data.Text}</td>;
        }
        
    }
}
