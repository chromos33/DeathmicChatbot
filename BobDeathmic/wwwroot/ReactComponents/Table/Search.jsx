class Search extends React.Component {
    constructor(props) {
        super(props);
        this.handleChange = this.handleChange.bind(this);
    }
    handleChange(e) {
        this.props.callback(e.target.value);
    }

    render() {
        return (
            <div className="tableSearch">
                <i className="fas fa-search"></i>
                <input type="text" name="search" onChange={this.handleChange} />
            </div>
            
        )
    }
}
