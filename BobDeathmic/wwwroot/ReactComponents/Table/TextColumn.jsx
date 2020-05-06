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
            return <span onClick={this.handleClick}>{this.props.data.Text}</span>;
        }
        else {
            return <span>{this.props.data.Text}</span>;
        }
        
    }
}
