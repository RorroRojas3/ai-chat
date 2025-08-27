export class FileUploadDto {
  id!: string;
  name!: string;
  size!: number;
  file!: File;
  status?: string;
  progress?: number;
}
