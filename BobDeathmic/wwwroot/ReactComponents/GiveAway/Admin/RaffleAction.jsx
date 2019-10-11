class RaffleAction extends React.Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
    }
    handleClick() {
        this.props.RaffleCall();
    }
    render() {
        return (
            <span onClick={this.handleClick} className="btn mb-4">Verlosen</span>
        );
    }
}
