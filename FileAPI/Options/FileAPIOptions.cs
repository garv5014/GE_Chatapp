﻿namespace FileAPI.Options;

public class FileAPIOptions
{
  public bool CompressImages { get; set; }
  public int APIDelayInSeconds { get; set; } = 1;
  public required string ServiceName { get; set; }
}