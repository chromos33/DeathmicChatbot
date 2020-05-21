class StreamsubAddComponent extends React.Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.state = {Options: [],Value: 0};
    }
    componentWillMount() {
        var thisreference = this;
        const xhr = new XMLHttpRequest();
        xhr.open('GET', "/User/SubscribableStreamsData", true);
        xhr.onload = function () {
            thisreference.setState({ Options: JSON.parse(xhr.responseText) });
        };
        xhr.send();
    }
    handleClick(e) {
        
        if (this.state.Value > 0) {
            var thisreference = this;
            const xhr = new XMLHttpRequest();
            xhr.open('GET', "/User/AddSubscription?streamid=" + this.state.Value, true);
            xhr.onload = function () {
                window.dispatchEvent(new Event('updateTable'));
                var newoptions = [];
                thisreference.state.Options.forEach((option) => {
                    if (option.StreamID != thisreference.state.Value) {
                        newoptions.push(option);
                    }
                });
                thisreference.setState({ Options: newoptions });

            };
            xhr.send();
        }
    }
    handleChange(e) {
        this.setState({Value:e.target.value});
    }
    render() {
        if (this.state.Options.length > 0) {
            var i = 0;
            const options = this.state.Options.map((option) => {
                i++;
                return (<option key={i} value={option.StreamID}>{option.Name}</option>)
            });
            return (
                <div>
                    <select onChange={this.handleChange}>
                        <option>Auswählen</option>
                        {options}
                    </select>
                    <button onClick={this.handleClick} className="btn btn-default">Subscription hinzufügen</button>
                </div>
                );
        }
        else {
            return (null);
        }
    }
}