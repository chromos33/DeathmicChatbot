class ChannelSelector extends React.Component {
    constructor(props) {
        super(props);
        this.state = { activeChannel: props.Channels[0] };
        this.onChange = this.onChange.bind(this);
        this.getOptions = this.getOptions.bind(this);
    }
    onChange(e) {
        let index = e.nativeEvent.target.selectedIndex;
        this.setState({ activeChannel: this.props.Channels[index] });
        this.props.changeSelectedChannel(this.props.Channels[index]);
    }
    getOptions() {
        let channelcount = 0;
        const channels = this.props.Channels.map(function (item) {
            channelcount++;
            return (
                <option value={item} key={channelcount} > {item}</ option>
            );
        });
        return channels;
    }
    render() {
        
        return (
            <div>
                <h2>Ausgabe Channel</h2>
                <select onChange={this.onChange} value={this.state.activeChannel}>
                    {this.getOptions()}
                </select>
            </div>
        );
    }
}
