/* eslint-disable no-param-reassign */
import { useEffect, useRef, useState } from 'react';

import ReactCrop, { Crop, PixelCrop } from 'react-image-crop';

import 'react-image-crop/dist/ReactCrop.css';

export function TokenCreator() {
	const [imgSrc, setImgSrc] = useState<string>('');
	const previewCanvasRef = useRef<HTMLCanvasElement>(null);
	const imgRef = useRef<HTMLImageElement>();
	const [crop, setCrop] = useState<Crop>();
	const [completedCrop, setCompletedCrop] = useState<PixelCrop>();
	const [rotate, setRotate] = useState(0);
	const [color, setColor] = useState('rebeccapurple');

	useEffect(() => {
		document.addEventListener('paste', onPaste);
		return () => document.removeEventListener('paste', onPaste);
		function onPaste(event: ClipboardEvent) {
			const { items } = event.clipboardData!;
			for (let index = 0; index < items.length; index += 1) {
				const item = items[index];
				if (item.kind === 'file') {
					const file = item.getAsFile();
					if (file) {
						loadFile(file);
					}
					break;
				}
			}
		}
	});

	useEffect(
		() => {
			if (completedCrop?.width && completedCrop?.height && imgRef.current && previewCanvasRef.current) {
				// We use canvasPreview as it's much faster than imgPreview.
				canvasPreview(imgRef.current, previewCanvasRef.current, completedCrop, rotate, color);
			}
		},
		// 100,
		[completedCrop, rotate, color]
	);

	return (
		<div className="App">
			<div className="Crop-Controls">
				<input type="file" accept="image/*" onChange={onSelectFile} />
				<div>
					<label htmlFor="rotate-input">Rotate: </label>
					<input
						id="rotate-input"
						type="number"
						value={rotate}
						disabled={!imgSrc}
						onChange={(e) => setRotate(Math.min(180, Math.max(-180, Number(e.target.value))))}
					/>
				</div>
				<div>
					<label htmlFor="color-input">Color: </label>
					<input id="color-input" type="text" value={color} onChange={(e) => setColor(e.target.value)} />
				</div>
			</div>
			{imgSrc && (
				<ReactCrop
					crop={crop ?? { aspect: 1 }}
					onImageLoaded={(img) => {
						imgRef.current = img;
					}}
					onChange={setCrop}
					onComplete={(c) => setCompletedCrop(c as PixelCrop)}
					src={imgSrc}
					imageStyle={{ maxHeight: '50vh', maxWidth: '50vw' }}
					circularCrop
				/>
			)}
			<div>
				{completedCrop && (
					<button type="button" onClick={copyUrl}>
						<canvas
							ref={previewCanvasRef}
							style={{
								border: '1px solid black',
								objectFit: 'contain',
								width: 400,
								height: 400,
							}}
						/>
					</button>
				)}
			</div>
		</div>
	);

	function onSelectFile(e: React.ChangeEvent<HTMLInputElement>) {
		if (e.target.files && e.target.files.length > 0) {
			setCrop(undefined); // Makes crop preview update between images.
			loadFile(e.target.files[0]);
		}
	}

	function loadFile(file: File) {
		const reader = new FileReader();
		reader.onload = () => {
			setImgSrc(reader.result as string);
		}; // data url!
		reader.readAsDataURL(file);
	}

	function copyUrl() {
		if (!previewCanvasRef.current) return;
		navigator.clipboard.writeText(previewCanvasRef.current.toDataURL());
	}
}

const TO_RADIANS = Math.PI / 180;

async function canvasPreview(
	image: HTMLImageElement,
	canvas: HTMLCanvasElement,
	crop: PixelCrop,
	rotate = 0,
	color = 'rebeccapurple'
) {
	const ctx = canvas.getContext('2d');

	if (!ctx) {
		throw new Error('No 2d context');
	}

	const scaleX = image.naturalWidth / image.width;
	const scaleY = image.naturalHeight / image.height;
	// devicePixelRatio slightly increases sharpness on retina devices
	// at the expense of slightly slower render times and needing to
	// size the image back down if you want to download/upload and be
	// true to the images natural size.
	const pixelRatio = window.devicePixelRatio;
	// const pixelRatio = 1

	canvas.width = Math.floor(crop.width * scaleX * pixelRatio);
	canvas.height = Math.floor(crop.height * scaleY * pixelRatio);

	// ctx.scale(pixelRatio, pixelRatio);
	ctx.imageSmoothingQuality = 'high';

	ctx.beginPath();
	ctx.ellipse(canvas.width / 2, canvas.width / 2, canvas.width / 2, canvas.width / 2, 0, 0, Math.PI * 2);
	ctx.fillStyle = color;
	ctx.fill();

	ctx.save();

	ctx.translate(canvas.width / 2, canvas.height / 2);
	ctx.rotate(rotate * TO_RADIANS);
	ctx.scale(0.95, 0.95);
	ctx.translate(-canvas.width / 2, -canvas.height / 2);

	ctx.beginPath();
	ctx.ellipse(canvas.width / 2, canvas.width / 2, canvas.width / 2, canvas.width / 2, 0, 0, Math.PI * 2);
	ctx.clip();

	ctx.drawImage(
		image,
		crop.x * scaleX,
		crop.y * scaleY,
		crop.width * scaleX,
		crop.height * scaleY,
		0,
		0,
		canvas.width,
		canvas.height
	);

	ctx.restore();
}

// Returns an image source you should set to state and pass
// `{previewSrc && <img alt="Crop preview" src={previewSrc} />}`
async function imgPreview(image: HTMLImageElement, crop: PixelCrop, rotate = 0, color = 'rebeccapurple') {
	const canvas = document.createElement('canvas');
	canvasPreview(image, canvas, crop, rotate, color);

	return canvas.toDataURL();
}
