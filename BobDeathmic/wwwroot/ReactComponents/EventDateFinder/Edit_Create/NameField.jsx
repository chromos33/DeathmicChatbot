class NameField extends React.Component {
    constructor(props) {
        super(props);
        this.state = { value: props.value, id: props.owner};
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handleFocus = this.handleFocus.bind(this);
        this.hangleOnChange = this.hangleOnChange.bind(this);
    }
    componentDidUpdate(prevstate) {
        if (prevstate.value === "" && this.props.value !== "") {
            this.setState({ value: this.props.value });
        }
        if (prevstate.id === "undefined") {
            this.setState({ id: this.props.owner });
        }
    }
    handleSubmit(event) {

    }
    hangleOnChange(event) {
        // update the state
        const inputName = event.target.value;
        this.setState({
            value: inputName
        });
    }
    handleFocus(event) {
        if (this.state.id !== undefined) {

            const Url = "/Events/UpdateEventTitle/";
            const data = new FormData();
            $.ajax({
                url: Url,
                type: "POST",
                data: { Title: this.state.value, ID: this.state.id},
                    success: function(result) {
                    }
            });
        }
        
    }
    render() {
        return (
            <input onBlur={this.handleFocus} onChange={this.hangleOnChange} className="NameField" type="text" value={this.state.value} />
        );
    }
}
