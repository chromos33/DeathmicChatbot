class UI extends React.Component {
    constructor(props) {
        super(props);
        this.state = { Game: "", Participants: [],Channels: [], CurrentChannel: "",CurrentWinners: [] };
        this.NextItemCall = this.NextItemCall.bind(this);
        this.RaffleCall = this.RaffleCall.bind(this);
        this.changeSelectedChannel = this.changeSelectedChannel.bind(this);
        this.UpdateParticipantList = this.UpdateParticipantList.bind(this);
    }
    componentDidMount() {
        //TODO: Initialize Data
        var request = new XMLHttpRequest();
        request.open("GET", "/GiveAway/InitialAdminData");
        var curthis = this;
        request.onload = function () {
            let data = JSON.parse(request.responseText);
            curthis.setState({
                Game: data.Item,
                Participants: data.Applicants,
                Channels: data.Channels,
                CurrentChannel: data.Channels[0]
            });
        };
        request.send();
        this.interval = setInterval(this.UpdateParticipantList, 5000);
    }
    componentWillUnmount() {
        clearInterval(this.interval);
    }
    UpdateParticipantList() {
        var request = new XMLHttpRequest();
        request.open("GET", "/GiveAway/UpdateParticipantList");
        var curthis = this;
        request.onload = function () {
            let data = JSON.parse(request.responseText);
            curthis.setState({
                Participants: data
            });
        };
        request.send();
    }
    NextItemCall() {
        var request = new XMLHttpRequest();
        request.open("GET", "/GiveAway/NextItem?channel=" + this.state.CurrentChannel, false);
        request.send(null);
        let data = JSON.parse(request.responseText);
        this.setState({
            Game: data.Item,
            Participants: data.Applicants
        });
    }
    RaffleCall() {
        var request = new XMLHttpRequest();
        request.open("GET", "/GiveAway/Raffle?channel=" + this.state.CurrentChannel, false);
        request.send(null);
        let data = JSON.parse(request.responseText);
        this.setState({ CurrentWinners: data });
    }
    changeSelectedChannel(e) {
        this.setState({ CurrentChannel: e});
    }
    render() {
        
        return (
            <div>
                <ChannelSelector changeSelectedChannel={this.changeSelectedChannel} Channels={this.state.Channels} />
                <NextItemAction NextItemCall={this.NextItemCall} />
                <CurrentItemInfo Game={this.state.Game} />
                <ParticipantList currentWinners={this.state.CurrentWinners} Participants={this.state.Participants} />
                <RaffleAction RaffleCall={this.RaffleCall} />
            </div>
        );
    }
}
