export interface TextAreaDefinition {
    x: number;
    y: number;
    width: number;
    height: number;
    fontSize: number;
    fontColor: string;
    borderSize: number;
    borderColor: string;
    isBold: boolean;
}

export interface MemeTemplate {
    id: string;
    title: string;
    imageUrl: string;
    textAreas: TextAreaDefinition[];
    createdAt: string;
    updatedAt?: string;
}

export interface MemeTemplatesListResponse {
    templates: MemeTemplate[];
}

export interface CreateMemeTemplateRequest {
    title: string;
    imageUrl: string;
    textAreas: TextAreaDefinition[];
}

export interface UpdateMemeTemplateRequest {
    title: string;
    imageUrl: string;
    textAreas: TextAreaDefinition[];
}

export interface UploadSasTokenResponse {
    blobUrl: string;
    sasToken: string;
    containerUrl: string;
    expiresAt: string;
}
