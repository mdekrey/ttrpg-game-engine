declare module 'worker-loader!*' {
	declare const worker: { new (): Worker };
	export default worker;
}
