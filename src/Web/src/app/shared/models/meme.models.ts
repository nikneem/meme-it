export interface MemeTextArea {
  id: string;
  x: number;
  y: number;
  width: number;
  height: number;
  fontFamily: string;
  fontSize: number;
  fontColor: string;
  maxLength: number;
}

export interface MemeTextAreaDto {
  x: number;
  y: number;
  width: number;
  height: number;
  fontFamily: string;
  fontSize: number;
  fontColor: string;
  maxLength: number;
}

export interface MediaDimensions {
  width: number;
  height: number;
}

export interface UploadedFile {
  file: File;
  url: string;
  type: string;
  dimensions?: MediaDimensions;
}

export interface CreateMemeRequest {
  name: string;
  description?: string;
  sourceImage: string;
  sourceWidth: number;
  sourceHeight: number;
  textareas: MemeTextAreaDto[];
}

export interface CreateMemeResponse {
  id: string;
  sourceImageUrl: string;
}

export interface GenerateUploadSasRequest {
  fileName: string;
  contentType: string;
}

export interface GenerateUploadSasResponse {
  sasUri: string;
  fileName: string;
  expiresAt: string;
}

export interface MemeTemplate {
  id: string;
  name: string;
  description?: string;
  sourceImageUrl: string;
  sourceWidth: number;
  sourceHeight: number;
  textAreas: MemeTextArea[];
}
