// /*!
//  * v2.0.0
//  * by rourou.lin
//  */

// /* tslint:disable:max-classes-per-file */
// import { HubConnectionBuilder, HubConnection, HttpTransportType } from '@aspnet/signalr';

// const hubStatus = Object.freeze({
// 	Initial: 0,
// 	Connected: 1,
// 	Disconnected: 2,
// 	Retry: 3,
// 	Error: 4,
// });

// class HubEvent {
// 	eventName: string;
// 	callback: () => void;
// 	constructor(eventName: string, callback: () => void) {
// 		this.eventName = eventName;
// 		this.callback = callback;
// 	}
// }

// class HubModel {
// 	name: string = '';
// 	url: string = '';
// 	connection: HubConnection | any;
// 	status: number = hubStatus.Initial;
// 	events: HubEvent[] = [];
// 	locker: boolean = false;

// 	constructor(name: string) {
// 		this.name = name;
// 	}

// 	async start(url: string, token: [REDACTED] Promise<void | never> {
// 		if (this.locker) {
// 			// console.debug('lock by another action...');
// 			return Promise.resolve();
// 		}

// 		this.locker = true;

// 		if (this.status === hubStatus.Connected) {
// 			// console.debug('already started => pass start...');
// 			return Promise.resolve();
// 		}

// 		if (this.status === hubStatus.Initial) {
// 			this.url = url;
// 			const options = {
// 				transport: HttpTransportType.WebSockets,
// 				skipNegotiation: true,
// 				accessTokenFactory() {
// 					return token;
// 				},
// 			};
// 			this.connection = new HubConnectionBuilder().withUrl(this.url, options).build();
// 		} else {
// 			this.clearEvent();
// 		}

// 		this.events.forEach(event => {
// 			this.connection.on(event.eventName, event.callback);
// 		});

// 		this.connection.onclose(() => {
// 			this.status = hubStatus.Disconnected;
// 		});

// 		await this.connection
// 			.start()
// 			.then(() => {
// 				this.status = hubStatus.Connected;
// 				this.locker = false;
// 			})
// 			.catch((err: any) => {
// 				this.status = hubStatus.Error;
// 				this.locker = false;
// 			});

// 		if (this.status === hubStatus.Connected) {
// 			return Promise.resolve();
// 		}

// 		return Promise.reject('signalr connection to server fail');
// 	}

// 	regist(event: HubEvent) {
// 		this.events.push(event);
// 	}

// 	clearEvent() {
// 		this.events.forEach(index => {
// 			this.connection.off(index.eventName);
// 		});
// 	}

// 	stop(): Promise<void> {
// 		if (this.status !== hubStatus.Connected) {
// 			// console.debug('connection not started => pass stop...');
// 			return Promise.resolve();
// 		}
// 		this.clearEvent();
// 		return this.connection.stop();
// 	}
// }

// const hubsPool: object | any = {};

// class WebsocketClient {
// 	get(name: string): HubModel {
// 		if (!hubsPool[name]) {
// 			hubsPool[name] = new HubModel(name);
// 		}
// 		return hubsPool[name];
// 	}
// }

// const client = new WebsocketClient();

// export class WebsocketRegister {
// 	hubName: string = '';
// 	constructor(hubName: string) {
// 		this.hubName = hubName;
// 	}
// 	set(eventName: string, callback: () => void): WebsocketRegister {
// 		client.get(this.hubName).regist(new HubEvent(eventName, callback));
// 		return this;
// 	}
// }

// export default client;
