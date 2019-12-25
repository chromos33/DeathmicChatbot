"use strict";

function _instanceof(left, right) { if (right != null && typeof Symbol !== "undefined" && right[Symbol.hasInstance]) { return !!right[Symbol.hasInstance](left); } else { return left instanceof right; } }

function _typeof(obj) { if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

function _classCallCheck(instance, Constructor) { if (!_instanceof(instance, Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _possibleConstructorReturn(self, call) { if (call && (_typeof(call) === "object" || typeof call === "function")) { return call; } return _assertThisInitialized(self); }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

function _assertThisInitialized(self) { if (self === void 0) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function"); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, writable: true, configurable: true } }); if (superClass) _setPrototypeOf(subClass, superClass); }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

var ChannelSelector =
    /*#__PURE__*/
    function (_React$Component) {
        _inherits(ChannelSelector, _React$Component);

        function ChannelSelector(props) {
            var _this;

            _classCallCheck(this, ChannelSelector);

            _this = _possibleConstructorReturn(this, _getPrototypeOf(ChannelSelector).call(this, props));
            _this.state = {
                activeChannel: props.Channels[0]
            };
            _this.onChange = _this.onChange.bind(_assertThisInitialized(_this));
            _this.getOptions = _this.getOptions.bind(_assertThisInitialized(_this));
            return _this;
        }

        _createClass(ChannelSelector, [{
            key: "onChange",
            value: function onChange(e) {
                var index = e.nativeEvent.target.selectedIndex;
                this.setState({
                    activeChannel: this.props.Channels[index]
                });
                this.props.changeSelectedChannel(this.props.Channels[index]);
            }
        }, {
            key: "getOptions",
            value: function getOptions() {
                var channelcount = 0;
                var channels = this.props.Channels.map(function (item) {
                    channelcount++;
                    return React.createElement("option", {
                        value: item,
                        key: channelcount
                    }, " ", item);
                });
                return channels;
            }
        }, {
            key: "render",
            value: function render() {
                return React.createElement("div", null, React.createElement("h2", null, "Ausgabe Channel"), React.createElement("select", {
                    onChange: this.onChange,
                    value: this.state.activeChannel
                }, this.getOptions()));
            }
        }]);

        return ChannelSelector;
    }(React.Component);

var CurrentItemInfo =
    /*#__PURE__*/
    function (_React$Component2) {
        _inherits(CurrentItemInfo, _React$Component2);

        function CurrentItemInfo(props) {
            _classCallCheck(this, CurrentItemInfo);

            return _possibleConstructorReturn(this, _getPrototypeOf(CurrentItemInfo).call(this, props));
        }

        _createClass(CurrentItemInfo, [{
            key: "render",
            value: function render() {
                var box;

                if (this.props.Game === "") {
                    box = "Test";
                } else {
                    box = this.props.Game;
                }

                return React.createElement("h2", {
                    className: "mb-4"
                }, React.createElement("a", {
                    href: this.props.Link
                }, box));
            }
        }]);

        return CurrentItemInfo;
    }(React.Component);

var NextItemAction =
    /*#__PURE__*/
    function (_React$Component3) {
        _inherits(NextItemAction, _React$Component3);

        function NextItemAction(props) {
            var _this2;

            _classCallCheck(this, NextItemAction);

            _this2 = _possibleConstructorReturn(this, _getPrototypeOf(NextItemAction).call(this, props));
            _this2.handleClick = _this2.handleClick.bind(_assertThisInitialized(_this2));
            return _this2;
        }

        _createClass(NextItemAction, [{
            key: "handleClick",
            value: function handleClick() {
                this.props.NextItemCall();
            }
        }, {
            key: "render",
            value: function render() {
                return React.createElement("span", {
                    onClick: this.handleClick,
                    className: "btn mb-4"
                }, "N\xE4chstes Spiel");
            }
        }]);

        return NextItemAction;
    }(React.Component);

var ParticipantList =
    /*#__PURE__*/
    function (_React$Component4) {
        _inherits(ParticipantList, _React$Component4);

        function ParticipantList(props) {
            _classCallCheck(this, ParticipantList);

            return _possibleConstructorReturn(this, _getPrototypeOf(ParticipantList).call(this, props));
        }

        _createClass(ParticipantList, [{
            key: "render",
            value: function render() {
                var key = 0;
                var curthis = this;
                var participants = this.props.Participants.map(function (item) {
                    key++;
                    return React.createElement("li", {
                        key: key
                    }, React.createElement("h5", null, item, curthis.props.currentWinners.includes(item) && React.createElement("i", {
                        style: {
                            color: "gold"
                        },
                        className: "fas fa-crown ml-2"
                    })));
                });
                return React.createElement("div", null, React.createElement("h2", null, "Teilnehmer"), React.createElement("ol", null, participants));
            }
        }]);

        return ParticipantList;
    }(React.Component);

var RaffleAction =
    /*#__PURE__*/
    function (_React$Component5) {
        _inherits(RaffleAction, _React$Component5);

        function RaffleAction(props) {
            var _this3;

            _classCallCheck(this, RaffleAction);

            _this3 = _possibleConstructorReturn(this, _getPrototypeOf(RaffleAction).call(this, props));
            _this3.handleClick = _this3.handleClick.bind(_assertThisInitialized(_this3));
            return _this3;
        }

        _createClass(RaffleAction, [{
            key: "handleClick",
            value: function handleClick() {
                this.props.RaffleCall();
            }
        }, {
            key: "render",
            value: function render() {
                return React.createElement("span", {
                    onClick: this.handleClick,
                    className: "btn mb-4"
                }, "Verlosen");
            }
        }]);

        return RaffleAction;
    }(React.Component);

var UI =
    /*#__PURE__*/
    function (_React$Component6) {
        _inherits(UI, _React$Component6);

        function UI(props) {
            var _this4;

            _classCallCheck(this, UI);

            _this4 = _possibleConstructorReturn(this, _getPrototypeOf(UI).call(this, props));
            _this4.state = {
                Game: "",
                Link: "",
                Participants: [],
                Channels: [],
                CurrentChannel: "",
                CurrentWinners: []
            };
            _this4.NextItemCall = _this4.NextItemCall.bind(_assertThisInitialized(_this4));
            _this4.RaffleCall = _this4.RaffleCall.bind(_assertThisInitialized(_this4));
            _this4.changeSelectedChannel = _this4.changeSelectedChannel.bind(_assertThisInitialized(_this4));
            _this4.UpdateParticipantList = _this4.UpdateParticipantList.bind(_assertThisInitialized(_this4));
            return _this4;
        }

        _createClass(UI, [{
            key: "componentDidMount",
            value: function componentDidMount() {
                //TODO: Initialize Data
                var request = new XMLHttpRequest();
                request.open("GET", "/GiveAway/InitialAdminData");
                var curthis = this;

                request.onload = function () {
                    var data = JSON.parse(request.responseText);
                    curthis.setState({
                        Game: data.Item,
                        Link: data.Link,
                        Participants: data.Applicants,
                        Channels: data.Channels,
                        CurrentChannel: data.Channels[0]
                    });
                };

                request.send();
                this.interval = setInterval(this.UpdateParticipantList, 5000);
            }
        }, {
            key: "componentWillUnmount",
            value: function componentWillUnmount() {
                clearInterval(this.interval);
            }
        }, {
            key: "UpdateParticipantList",
            value: function UpdateParticipantList() {
                var request = new XMLHttpRequest();
                request.open("GET", "/GiveAway/UpdateParticipantList");
                var curthis = this;

                request.onload = function () {
                    var data = JSON.parse(request.responseText);
                    curthis.setState({
                        Participants: data
                    });
                };

                request.send();
            }
        }, {
            key: "NextItemCall",
            value: function NextItemCall() {
                var request = new XMLHttpRequest();
                request.open("GET", "/GiveAway/NextItem?channel=" + this.state.CurrentChannel, false);
                request.send(null);
                var data = JSON.parse(request.responseText);
                this.interval = setInterval(this.UpdateParticipantList, 5000);
                this.setState({
                    Game: data.Item,
                    Link: data.Link,
                    Participants: data.Applicants,
                    CurrentWinners: []
                });
            }
        }, {
            key: "RaffleCall",
            value: function RaffleCall() {
                var request = new XMLHttpRequest();
                request.open("GET", "/GiveAway/Raffle?channel=" + this.state.CurrentChannel, false);
                request.send(null);
                var data = JSON.parse(request.responseText);
                this.setState({
                    CurrentWinners: data
                });
                clearInterval(this.interval);
            }
        }, {
            key: "changeSelectedChannel",
            value: function changeSelectedChannel(e) {
                this.setState({
                    CurrentChannel: e
                });
            }
        }, {
            key: "render",
            value: function render() {
                return React.createElement("div", null, React.createElement(ChannelSelector, {
                    changeSelectedChannel: this.changeSelectedChannel,
                    Channels: this.state.Channels
                }), React.createElement(NextItemAction, {
                    NextItemCall: this.NextItemCall
                }), React.createElement(CurrentItemInfo, {
                    Game: this.state.Game,
                    Link: this.state.Link
                }), React.createElement(ParticipantList, {
                    currentWinners: this.state.CurrentWinners,
                    Participants: this.state.Participants
                }), React.createElement(RaffleAction, {
                    RaffleCall: this.RaffleCall
                }));
            }
        }]);

        return UI;
    }(React.Component);