class StreamAddComponent extends React.Component {
    constructor(props) {
        super(props);
        this.handleSave = this.handleSave.bind(this);
        this.handleLoadCreateForm = this.handleLoadCreateForm.bind(this);
        this.handleCancel = this.handleCancel.bind(this);
        this.handleNameChange = this.handleNameChange.bind(this);
        this.handleUptimeChange = this.handleUptimeChange.bind(this);
        this.handleQuoteChange = this.handleQuoteChange.bind(this);
        this.channelswitch = this.channelswitch.bind(this);
        this.handleTypeClick = this.handleTypeClick.bind(this);
        this.state = {
            Open: false,
            CanSave: true,
            StreamName: "",
            Type: "",
            UpTime: 0,
            Quote: 0,
            Relay: "Aus",
            Channels: []
        };
    }
    handleSave(e) {
        if (this.state.CanSave) {
            var request = new XMLHttpRequest();
            request.open("POST", "/Stream/Create", true);
            var curthis = this;
            request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            request.onreadystatechange = function () { // Call a function when the state changes.
                if (this.readyState === XMLHttpRequest.DONE && this.status === 200) {
                    curthis.setState({ Open: false });
                    window.dispatchEvent(new Event('updateTable'));
                }
            }

            request.send("StreamName=" + this.state.StreamName + "&Type=" + this.state.Type + "&UpTime=" + this.state.UpTime + "&Quote=" + this.state.Quote + "&Relay=" + this.state.Relay);
        }
    }
    handleCancel(e) {
        this.setState({Open: false});
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
    handleUptimeChange(e) {
        this.setState({
            UpTime: e.target.value
        });
    }
    channelswitch(e) {
        let index = e.nativeEvent.target.selectedIndex;
        this.setState({ Relay: this.state.Channels[index] });
    }
    handleQuoteChange(e) {
        this.setState({
            Quote: e.target.value
        });
    }
    handleTypeClick(e) {
        this.setState({ Type: e.target.getAttribute("data-type") });
    }
    getTypeSwitchCSSClasses(type) {
        let classes = "typeswitch";
        if (type === this.state.Type) {
            classes += " active";
        }
        classes += " " + type;
        return classes;
    }
    handleLoadCreateForm(e) {
        const xhr = new XMLHttpRequest();
        var thisreference = this;
        xhr.open('GET', "/Stream/RelayChannels", true);
        xhr.onload = function () {
            if (xhr.responseText !== "") {
                let data = JSON.parse(xhr.responseText);
                console.log(data);
                
                thisreference.setState({
                    Open: true,
                    Channels: data
                });
            }
        };
        xhr.send();
        this.setState({Open: true});
    }
    render() {
        
        if (this.state.Open) {
            const options = this.state.Channels.map((e) => {
                return <option key={e} value={e}>{e}</option>
            })
            return (
                <div>
                    <span className="pointer btn btn_primary mb-3"><i className="fa fa-plus"></i> Hinzufügen</span>
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
                </div>
            );
        }
        else {
            return (<span onClick={this.handleLoadCreateForm} className="pointer btn btn_primary mb-3"><i className="fa fa-plus"></i> Hinzufügen</span>)
        }
    }
}