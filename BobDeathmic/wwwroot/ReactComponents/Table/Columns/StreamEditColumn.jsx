class StreamEditColumn extends React.Component {
    constructor(props) {
        super(props);
        this.handleTypeClick = this.handleTypeClick.bind(this);
        this.getTypeSwitchCSSClasses = this.getTypeSwitchCSSClasses.bind(this);
        this.handleNameChange = this.handleNameChange.bind(this);
        this.handleUptimeChange = this.handleUptimeChange.bind(this);
        this.handleQuoteChange = this.handleQuoteChange.bind(this);
        this.channelswitch = this.channelswitch.bind(this);
        this.handleSave = this.handleSave.bind(this);
        this.handleLoadEditForm = this.handleLoadEditForm.bind(this);
        this.handleCancel = this.handleCancel.bind(this);
        this.state = {
            Open: false,
            CanSave: true,
            StreamName: "",
            Type: "",
            UpTime: 0,
            Quote: 0,
            RelayChannel: "",
            Channels: []
        };
    }
    handleTypeClick(e) {
        this.setState({ Type: e.target.getAttribute("data-type") });
    }
    handleNameChange(e) {
        if (e.target.value === "") {
            this.setState({
                StreamName: e.target.value,
                CanSave: false
            });
        }
        else {
            this.setState({
                StreamName: e.target.value,
                CanSave: true
            });
        }
        
    }
    channelswitch(e) {
        let index = e.nativeEvent.target.selectedIndex;
        this.setState({ RelayChannel: this.state.Channels[index] });
    }
    handleUptimeChange(e) {
        this.setState({
            UpTime: e.target.value
        });
    }
    handleQuoteChange(e) {
        this.setState({
            Quote: e.target.value
        });
    }
    getTypeSwitchCSSClasses(type) {
        let classes = "typeswitch";
        if (type === this.state.Type) {
            classes += " active";
        }
        classes += " " + type;
        return classes;
    }
    handleSave(e) {
        if (this.state.CanSave) {
            var request = new XMLHttpRequest();
            request.open("POST", "/Stream/SaveStreamEdit", true);
            var curthis = this;
            request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            request.onreadystatechange = function () { // Call a function when the state changes.
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                    curthis.setState({ Open: false });
                    window.dispatchEvent(new Event('updateTable'));
                }
            }

            request.send("StreamID=" + this.props.data.StreamID + "&StreamName=" + this.state.StreamName + "&Type=" + this.state.Type + "&UpTime=" + this.state.UpTime + "&Quote=" + this.state.Quote + "&Relay=" + this.state.RelayChannel);
        }
    }

    handleCancel(e) {
        this.setState({Open: false});
    }
    handleLoadEditForm(e) {
        const xhr = new XMLHttpRequest();
        var thisreference = this;
        xhr.open('GET', "/Stream/GetStreamEditData?streamID="+this.props.data.StreamID, true);
        xhr.onload = function () {
            if (xhr.responseText !== "") {
                let data = JSON.parse(xhr.responseText);
                thisreference.setState({
                    Open: true,
                    StreamName: data.StreamName,
                    Type: data.Type.toLowerCase(),
                    UpTime: data.UpTime,
                    Quote: data.Quote,
                    Channels: data.RelayChannels,
                    RelayChannel: data.RelayChannel
                });
            }
        };
        xhr.send();
    }
    render() {
        if (this.state.Open) {
            const options = this.state.Channels.map((e) => {
                return <option value={e}>{e}</option>
            })
            return <td>
                {this.props.data.Text}
                <div className="shadowlayer"></div>
                <div className="statictest grid column-5 row-11">
                    <label className="namelabel">Streamname</label>
                    <input className="namefield" name="streamname" value={this.state.StreamName} onChange={this.handleNameChange} type="text" />
                    <label className="typelabel">Type</label>
                    <span data-type="twitch" onClick={this.handleTypeClick} className={this.getTypeSwitchCSSClasses("twitch")}>Twitch</span>
                    <span data-type="dlive" onClick={this.handleTypeClick} className={this.getTypeSwitchCSSClasses("dlive")}>D-Live</span>
                    <span data-type="mixer" onClick={this.handleTypeClick} className={this.getTypeSwitchCSSClasses("mixer")}>Mixer</span>
                    <div className="grid row-2 column-2 timergrid">
                        <label className="uptimelabel">Uptime Interval</label>
                        <input value={this.state.UpTime} onChange={this.handleUptimeChange} className="uptimefield" type="number" />
                        <label className="quotelabel">Quote Interval</label>
                        <input value={this.state.Quote} onChange={this.handleQuoteChange} className="quotefield" type="number" />
                    </div>
                    <label className="relaylabel">Relay</label>
                    <select className="relayselect" value={this.state.RelayChannel} onChange={this.channelswitch}>{options}</select>
                    <span onClick={this.handleSave} className="btn btn_primary savebtn">Speichern</span>
                    <span onClick={this.handleCancel} className="btn btn_primary cancelbtn">Abbrechen</span>
                </div>
            </td>;
        }
        else {
            return <td className="pointer" onClick={this.handleLoadEditForm}>
                {this.props.data.Text}
            </td>;
        }
       

    }
}
